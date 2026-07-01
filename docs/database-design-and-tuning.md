# CanvaSync Database Design And Tuning Notes

This note explains the database decisions that are visible in code and can be
verified with PostgreSQL query plans.

## Access Patterns

CanvaSync uses PostgreSQL for durable data and Redis/InMemory storage for the
live drawing state. The important persistent access patterns are:

- Login: find one member by `Members.Name`.
- Lecture join: find one lecture by the 6-digit `Lectures.Code`.
- My page: list lectures hosted by or joined by the current member.
- Drawing load/save: read or upsert one `DrawingData` row by `(LectureId, MemberId)`.
- Cleanup: delete drawing data automatically when the owning lecture or member is deleted.

## Constraints And Indexes

The schema now lets the database enforce the same rules that the application
expects:

- `Members.Name` is unique, matching login and registration lookup.
- `Lectures.Code` is unique, so two active lectures cannot share the same join code.
- `DrawingData(LectureId, MemberId)` is unique, so one user has one durable drawing snapshot per lecture.
- `DrawingData.LectureId` and `DrawingData.MemberId` are foreign keys with cascade delete.

The `DrawingData` unique index is also the hot-path lookup index for drawing
load/save. It supports queries like:

```sql
SELECT *
FROM "DrawingData"
WHERE "LectureId" = $1 AND "MemberId" = $2;
```

## Query Changes

The service layer avoids loading full tracked graphs for read-only screens:

- Lecture list queries use `AsNoTracking()`.
- Drawing reads use the `(LectureId, MemberId)` predicate directly.
- Drawing saves use `ExecuteUpdateAsync()` first, then insert, then retry update on unique-conflict races.

That save flow avoids the old "SELECT first, then UPDATE or INSERT" pattern on
the normal update path.

## Verification

Use `docs/sql/lecture-access-tuning.sql` against a local PostgreSQL database to
check that the main lookups use index scans. Run it after applying migrations:

```bash
dotnet ef database update --project canvasync/canvasync.csproj --startup-project canvasync/canvasync.csproj
psql "$CANVASYNC_DATABASE_URL" -f docs/sql/lecture-access-tuning.sql
```

For a portfolio or interview, capture the `EXPLAIN (ANALYZE, BUFFERS)` output
before and after the index migration. The expected improvement is most visible
on `DrawingData` lookup and `Lectures.Code` lookup once the tables have enough
rows to make sequential scans expensive.
