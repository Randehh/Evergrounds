using System.Collections.Generic;

public class InputStateService : IService
{

    public InputState InputState { get; set; } = InputState.WORLD;

    private Stack<InputState> inputStack = new();

    public void OnInit()
    {
    }

    public void OnReady()
    {
    }

    public void OnDestroy()
    {
    }

    public void PushInputState(InputState state)
    {
        inputStack.Push(state);

        ServiceLocator.GameNotificationService.OnInputStateChanged.Execute(state);
    }

    public void PopInputState()
    {
        if(inputStack.Count > 0)
        {
            inputStack.Pop();
        }

        // Default state
        InputState nextState = InputState.WORLD;
        if(inputStack.TryPeek(out InputState peekState))
        {
            nextState = peekState;
        }

        ServiceLocator.GameNotificationService.OnInputStateChanged.Execute(nextState);
    }
}

public enum InputState
{
    WORLD,
    UI
} 