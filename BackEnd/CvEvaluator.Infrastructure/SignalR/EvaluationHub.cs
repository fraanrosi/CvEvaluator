using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CvEvaluator.Infrastructure.SignalR;

[Authorize]
public class EvaluationHub : Hub
{
}
