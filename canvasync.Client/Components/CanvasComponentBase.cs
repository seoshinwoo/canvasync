using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace canvasync.Client.Components;

public class CanvasComponentBase : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal? User { get; private set; }
    protected string? MemberId { get; private set; }
    protected string? MemberName { get; private set; }
    protected bool IsLoggedIn => User?.Identity?.IsAuthenticated ?? false;

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateTask != null)
        {
            var authState = await AuthenticationStateTask;
            User = authState.User;

            if (IsLoggedIn)
            {
                MemberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                MemberName = User.Identity?.Name;
            }
        }

        await base.OnInitializedAsync();
    }
}
