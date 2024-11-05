public interface IException
{
    void HandleException();
    string GetMessage();
    string GetCode();
}