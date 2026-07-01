-- CanvaSync PostgreSQL query-plan checks.
-- Replace the sample values with IDs/codes from your local database.

\echo '1. Login lookup by unique member name'
EXPLAIN (ANALYZE, BUFFERS)
SELECT "Id", "Name", "Password"
FROM "Members"
WHERE "Name" = 'sample-user';

\echo '2. Lecture lookup by unique 6-digit code'
EXPLAIN (ANALYZE, BUFFERS)
SELECT "Id", "Code", "FileName", "PdfFileAddress", "HostMemberId"
FROM "Lectures"
WHERE "Code" = '123456';

\echo '3. Drawing snapshot lookup by lecture and member'
EXPLAIN (ANALYZE, BUFFERS)
SELECT "Id", "LectureId", "MemberId", "Drawings"
FROM "DrawingData"
WHERE "LectureId" = 'replace-with-lecture-id'
  AND "MemberId" = 'replace-with-member-id';

\echo '4. Hosted lectures for My Page'
EXPLAIN (ANALYZE, BUFFERS)
SELECT l."Id", l."Code", l."FileName", l."PdfFileAddress", l."HostMemberId"
FROM "Lectures" l
WHERE l."HostMemberId" = 'replace-with-member-id'
ORDER BY l."FileName";

\echo '5. Joined lectures for My Page'
EXPLAIN (ANALYZE, BUFFERS)
SELECT l."Id", l."Code", l."FileName", l."PdfFileAddress", l."HostMemberId"
FROM "Lectures" l
WHERE EXISTS (
    SELECT 1
    FROM "LectureMember" lm
    WHERE lm."JoinedLecturesId" = l."Id"
      AND lm."MembersId" = 'replace-with-member-id'
)
ORDER BY l."FileName";
