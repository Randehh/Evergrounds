
public interface IService
{
    void OnInit();
    void OnReady();
    void OnDestroy();
    void Process(double delta) { }
}