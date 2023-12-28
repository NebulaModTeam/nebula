using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NebulaAPI.Simulation;

namespace NebulaAPI.Tasks;

public enum ETaskType
{
    OneTime = 0,
    Repeating,
    Endless
}

/// <summary>
/// Base functionality for nebula jobs.
/// Bound to the cancellation token of an IJobTicker.
/// Runs on the game thread.
/// </summary>
public class NebulaTask : INebulaTask
{
    public ISimulationTicker SimulationTicker { get; }

    private readonly CancellationToken cancellationToken;

    private readonly TaskCompletionSource<bool> taskCompletionSource = new();

    public ETaskType TaskType { get; }

    /// <summary>
    /// How long to wait before triggering for the first time.
    /// Setting this to 0 means this task will execute on the next update.
    /// </summary>
    public float TriggerInterval { get; }

    /// <summary>
    /// Amount of times this task has executed.
    /// </summary>
    public int NumExecutions { get; protected set; }

    /// <summary>
    /// Amount of time elapsed since the last time we were triggered
    /// </summary>
    protected float timeElapsed;

    protected int NumRepeats { get; }

    protected Action executeAction;

    protected Action completionAction;

    protected Action cancelOrErrorAction;

    protected Func<bool> conditionFunc = () => true;

    /// <summary>
    /// </summary>
    /// <param name="simulationTicker"></param>
    /// <param name="triggerInterval">Amount of time to wait before triggering.
    /// Defaults to 0, meaning the task executes on the first update it receives.</param>
    public NebulaTask(ISimulationTicker simulationTicker, float triggerInterval = 0f)
    {
        SimulationTicker = simulationTicker;
        TriggerInterval = triggerInterval;
        TaskType = ETaskType.OneTime;
        cancellationToken = simulationTicker.CancellationToken;
    }

    /// <summary>
    /// </summary>
    /// <param name="simulationTicker"></param>
    /// <param name="triggerInterval">Amount of time to wait before triggering.
    /// Defaults to 0, meaning the task executes on the first update it receives.</param>
    /// <param name="isEndless">Set to true if you want this task to keep executing until cancelled or the application exits.</param>
    public NebulaTask(ISimulationTicker simulationTicker, bool isEndless, float triggerInterval = 0f) : this(simulationTicker,
        triggerInterval)
    {
        TaskType = isEndless ? ETaskType.Endless : ETaskType.OneTime;
    }

    /// <summary>
    /// </summary>
    /// <param name="simulationTicker"></param>
    /// <param name="numRepeats">Number of time to repeat</param>
    /// <param name="triggerInterval">Amount of time to wait before triggering.
    /// Defaults to 0, meaning the task executes on the first update it receives.</param>
    public NebulaTask(ISimulationTicker simulationTicker, int numRepeats, float triggerInterval = 0f) : this(simulationTicker,
        triggerInterval)
    {
        NumRepeats = numRepeats;
        TaskType = ETaskType.Repeating;
    }

    ~NebulaTask()
    {
        var test = new Task(() => { });
        Cancel();
    }

    public bool IsRunning { get; private set; }

    public bool IsCancelled { get; private set; }

    /// <returns>True if task has fully finished it's execution.</returns>
    public virtual bool IsCompleted { get; private set; }

    private void OnUpdate()
    {
        if (cancellationToken.IsCancellationRequested)
            Cancel();

        timeElapsed += SimulationTickTime.DeltaTime;

        var shouldExecute = timeElapsed >= TriggerInterval;
        if (!shouldExecute)
            return;

        // When it's time to trigger, we check the condition provided by the user if any, and execute if it's true.
        timeElapsed -= TriggerInterval;

        var conditionMet = conditionFunc.Invoke();
        if (conditionMet)
            ExecuteInternal();
    }

    protected virtual void ExecuteInternal()
    {
        try
        {
            executeAction?.Invoke();
        }
        catch (Exception e)
        {
            cancelOrErrorAction?.Invoke();
            taskCompletionSource.SetResult(false);
            throw;
        }

        NumExecutions++;

        switch (TaskType)
        {
            case ETaskType.OneTime:
                Complete();
                break;
            case ETaskType.Repeating:
                if (NumExecutions >= NumRepeats)
                    Complete();
                break;
            case ETaskType.Endless:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public NebulaTask Start()
    {
        if (IsCompleted || IsCancelled || IsRunning)
            throw new InvalidOperationException("Cannot start a running, completed or cancelled task!");

        IsRunning = true;
        SimulationTicker.Ticked += OnUpdate;
        return this;
    }

    public void Cancel()
    {
        SimulationTicker.Ticked -= OnUpdate;
        IsCancelled = true;
        cancelOrErrorAction?.Invoke();
        taskCompletionSource.SetResult(false);
    }

    private void Complete()
    {
        SimulationTicker.Ticked -= OnUpdate;
        IsCompleted = true;
        completionAction?.Invoke();
        taskCompletionSource.SetResult(true);
    }

    /// <summary>
    /// Sets a condition to check against before executing, if it evaluates to true, the task executes & updates it's state.
    /// Execution counters - such as NumRepeats - will not increment when this evaluates to false. 
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public NebulaTask SetCondition(Func<bool> condition)
    {
        conditionFunc = condition ?? throw new NullReferenceException();
        return this;
    }

    /// <summary>
    /// Sets the action to perform when this task executes.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public NebulaTask OnExecuted(Action action)
    {
        if (executeAction is not null)
            throw new InvalidOperationException("The task already has a completion action.");

        executeAction = action;
        return this;
    }

    /// <summary>
    /// Sets the action to perform after this task completes.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public NebulaTask OnCompleted(Action action)
    {
        if (completionAction is not null)
            throw new InvalidOperationException("The task already has a completion action.");

        completionAction = action;
        return this;
    }

    /// <summary>
    /// Sets the action to perform in case cancellation was requested, or an error occurs.
    /// Will perform the action, then re-throw any exceptions caught.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public NebulaTask OnCancelOrError(Action action)
    {
        if (cancelOrErrorAction is not null)
            throw new InvalidOperationException("The task already has a completion action.");

        cancelOrErrorAction = action;
        return this;
    }

    // @Todo: find a way to wait without blocking the game thread. 
    private void Wait()
    {
        throw new NotImplementedException();
        taskCompletionSource.Task.Wait();
    }

    // @Todo: Find a way to wait without blocking the game thread.
    private static void WaitAll(IEnumerable<NebulaTask> tasks)
    {
        throw new NotImplementedException();
        foreach (var task in tasks)
        {
            task.Wait();
        }
    }
}
