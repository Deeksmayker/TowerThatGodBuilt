namespace Source.Features.SceneEditor.Interfaces
{
    public interface IChangeStateListener<T>
    {
        void OnStateChange(T state);
    }
}