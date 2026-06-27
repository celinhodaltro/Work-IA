using Work_IA.Application.Common.Events;

namespace Work_IA.Application.Common.Interfaces;

public interface IFileChangeObservable
{
    IObservable<WorkspaceFileChange> FileChanges { get; }
}
