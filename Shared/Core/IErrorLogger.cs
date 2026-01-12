using System.Diagnostics.CodeAnalysis;
namespace GERMAG.Shared.Core;

public interface IErrorLogger
{
    [DoesNotReturn()]
    Task LogError(string error);

    Task Prompt(string message, string title);

    ValueTask<bool> Confirm(string message, string title);
}
