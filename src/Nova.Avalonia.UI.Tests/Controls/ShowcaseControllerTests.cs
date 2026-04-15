using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShowcaseControllerTests
{
    [AvaloniaFact]
    public void Controller_Should_Have_Default_Values()
    {
        var controller = new ShowcaseController();

        Assert.Empty(controller.Steps);
        Assert.Null(controller.CurrentStep);
        Assert.Equal(-1, controller.CurrentIndex);
        Assert.False(controller.IsActive);
        Assert.False(controller.CanGoPrevious);
        Assert.False(controller.CanGoNext);
    }

    [AvaloniaFact]
    public void Start_Should_Set_CurrentIndex_To_Zero()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        controller.Start();

        Assert.Equal(0, controller.CurrentIndex);
        Assert.True(controller.IsActive);
        Assert.NotNull(controller.CurrentStep);
        Assert.Equal("Step1", controller.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Start_Should_Do_Nothing_If_No_Steps()
    {
        var controller = new ShowcaseController();

        controller.Start();

        Assert.Equal(-1, controller.CurrentIndex);
        Assert.False(controller.IsActive);
    }

    [AvaloniaFact]
    public void Start_While_Active_Should_Restart_From_Beginning()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();
        controller.Next();

        Assert.Equal(1, controller.CurrentIndex);

        var startedCount = 0;
        controller.Started += (s, e) => startedCount++;

        controller.Start();

        Assert.Equal(0, controller.CurrentIndex);
        Assert.True(controller.IsActive);
        Assert.Equal(1, startedCount);
    }

    [AvaloniaFact]
    public void Next_Should_Advance_To_Next_Step()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();

        controller.Next();

        Assert.Equal(1, controller.CurrentIndex);
        Assert.Equal("Step2", controller.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Next_On_Last_Step_Should_Complete()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        var completed = false;
        controller.Completed += (s, e) => completed = true;

        controller.Next();

        Assert.True(completed);
        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void Next_When_Not_Active_Should_Do_Nothing()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        controller.Next();

        Assert.Equal(-1, controller.CurrentIndex);
        Assert.False(controller.IsActive);
    }

    [AvaloniaFact]
    public void Previous_Should_Go_Back()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();
        controller.Next();

        controller.Previous();

        Assert.Equal(0, controller.CurrentIndex);
        Assert.Equal("Step1", controller.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Previous_On_First_Step_Should_Do_Nothing()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        controller.Previous();

        Assert.Equal(0, controller.CurrentIndex);
        Assert.False(controller.CanGoPrevious);
    }

    [AvaloniaFact]
    public void Previous_When_Not_Active_Should_Do_Nothing()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });

        controller.Previous();

        Assert.Equal(-1, controller.CurrentIndex);
        Assert.False(controller.IsActive);
    }

    [AvaloniaFact]
    public void Skip_Should_End_Showcase()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();

        var skipped = false;
        controller.Skipped += (s, e) => skipped = true;

        controller.Skip();

        Assert.True(skipped);
        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void Skip_When_Not_Active_Should_Do_Nothing()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        var skipped = false;
        controller.Skipped += (s, e) => skipped = true;

        controller.Skip();

        Assert.False(skipped);
    }

    [AvaloniaFact]
    public void CanGoPrevious_Should_Be_True_After_First_Step()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();

        Assert.False(controller.CanGoPrevious);

        controller.Next();

        Assert.True(controller.CanGoPrevious);
    }

    [AvaloniaFact]
    public void CanGoNext_Should_Be_False_On_Last_Step()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();

        Assert.True(controller.CanGoNext);

        controller.Next();

        Assert.False(controller.CanGoNext);
    }

    [AvaloniaFact]
    public void CurrentButtonText_Should_Change_On_Last_Step()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();

        Assert.Equal("Next", controller.CurrentButtonText);

        controller.Next();

        Assert.Equal("Finish", controller.CurrentButtonText);
    }

    [AvaloniaFact]
    public void Custom_Button_Texts_Should_Be_Used()
    {
        var controller = new ShowcaseController
        {
            NextButtonText = "Siguiente",
            FinishButtonText = "Finalizar",
            PreviousButtonText = "Anterior",
            SkipButtonText = "Omitir"
        };
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();

        Assert.Equal("Siguiente", controller.CurrentButtonText);
        Assert.Equal("Anterior", controller.PreviousButtonText);
        Assert.Equal("Omitir", controller.SkipButtonText);

        controller.Next();

        Assert.Equal("Finalizar", controller.CurrentButtonText);
    }

    [AvaloniaFact]
    public void StepChanged_Event_Should_Fire()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });

        ShowcaseStepChangedEventArgs? args = null;
        controller.StepChanged += (s, e) => args = e;

        controller.Start();

        Assert.NotNull(args);
        Assert.Null(args!.PreviousStep);
        Assert.Equal("Step1", args.CurrentStep.Key);
        Assert.Equal(0, args.CurrentIndex);

        controller.Next();

        Assert.Equal("Step1", args.PreviousStep!.Key);
        Assert.Equal("Step2", args.CurrentStep.Key);
        Assert.Equal(1, args.CurrentIndex);
    }

    [AvaloniaFact]
    public void Started_Event_Should_Fire()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        var started = false;
        controller.Started += (s, e) => started = true;

        controller.Start();

        Assert.True(started);
    }

    [AvaloniaFact]
    public void Reset_Should_Clear_State()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        controller.Reset();

        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
        Assert.Null(controller.CurrentStep);
    }

    [AvaloniaFact]
    public void NextCommand_CanExecute_Should_Reflect_IsActive()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        Assert.False(controller.NextCommand.CanExecute(null));

        controller.Start();

        Assert.True(controller.NextCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void PreviousCommand_CanExecute_Should_Reflect_State()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });

        Assert.False(controller.PreviousCommand.CanExecute(null));

        controller.Start();
        Assert.False(controller.PreviousCommand.CanExecute(null));

        controller.Next();
        Assert.True(controller.PreviousCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public async Task StartAsync_Should_Invoke_Step_Hooks()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        ShowcaseStepTransitionContext? beforeContext = null;
        ShowcaseStepTransitionContext? afterContext = null;
        controller.BeforeStepAsync = (context, _) =>
        {
            beforeContext = context;
            return Task.CompletedTask;
        };
        controller.AfterStepAsync = (context, _) =>
        {
            afterContext = context;
            return Task.CompletedTask;
        };

        await controller.StartAsync();

        Assert.NotNull(beforeContext);
        Assert.NotNull(afterContext);
        Assert.Equal(ShowcaseStepTransitionReason.Start, beforeContext!.Reason);
        Assert.Equal("Step1", beforeContext.NextStep.Key);
        Assert.Null(beforeContext.PreviousStep);
        Assert.Equal(0, afterContext!.NextIndex);
        Assert.Equal(0, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task NextAsync_Should_Invoke_Hooks_With_Next_Context()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        await controller.StartAsync();

        ShowcaseStepTransitionContext? beforeContext = null;
        controller.BeforeStepAsync = (context, _) =>
        {
            beforeContext = context;
            return Task.CompletedTask;
        };

        await controller.NextAsync();

        Assert.NotNull(beforeContext);
        Assert.Equal(ShowcaseStepTransitionReason.Next, beforeContext!.Reason);
        Assert.Equal("Step1", beforeContext.PreviousStep!.Key);
        Assert.Equal("Step2", beforeContext.NextStep.Key);
        Assert.Equal(0, beforeContext.PreviousIndex);
        Assert.Equal(1, beforeContext.NextIndex);
    }

    [AvaloniaFact]
    public async Task ResumeAsync_Should_Restore_Persisted_Progress()
    {
        var store = new InMemoryShowcaseProgressStore();
        var controller = CreatePersistentController(store, "showcase-resume");

        await controller.StartAsync();
        await controller.NextAsync();

        var resumedController = CreatePersistentController(store, "showcase-resume");
        var resumed = await resumedController.ResumeAsync();

        Assert.True(resumed);
        Assert.True(resumedController.IsActive);
        Assert.Equal(1, resumedController.CurrentIndex);
        Assert.Equal("Step2", resumedController.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public async Task ResumeAsync_Should_Return_False_When_No_Persisted_Progress_Exists()
    {
        var controller = CreatePersistentController(new InMemoryShowcaseProgressStore(), "missing");

        var resumed = await controller.ResumeAsync();

        Assert.False(resumed);
        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task SkipAsync_Should_Clear_Persisted_Progress()
    {
        var store = new InMemoryShowcaseProgressStore();
        var controller = CreatePersistentController(store, "showcase-skip");

        await controller.StartAsync();
        await controller.SkipAsync();

        var resumedController = CreatePersistentController(store, "showcase-skip");
        var resumed = await resumedController.ResumeAsync();

        Assert.False(resumed);
    }

    [AvaloniaFact]
    public async Task Newer_Transition_Should_Cancel_InFlight_Hook()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });

        var nextHookStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        controller.BeforeStepAsync = (context, cancellationToken) =>
        {
            if (context.Reason != ShowcaseStepTransitionReason.Next)
                return Task.CompletedTask;

            nextHookStarted.TrySetResult();
            return Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        };

        await controller.StartAsync();

        var nextTask = controller.NextAsync();
        await nextHookStarted.Task;
        await controller.SkipAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await nextTask);
        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task Sync_Start_Should_Raise_TransitionFailed_When_Hook_Throws()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.BeforeStepAsync = (_, _) => throw new InvalidOperationException("boom");

        var failure = new TaskCompletionSource<ShowcaseTransitionFailedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        controller.TransitionFailed += (_, args) => failure.TrySetResult(args);

        controller.Start();

        var args = await failure.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal(ShowcaseNavigationAction.Start, args.Action);
        Assert.IsType<InvalidOperationException>(args.Exception);
        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task ResumeAsync_Should_Clear_Invalid_Persisted_Index()
    {
        var store = new InMemoryShowcaseProgressStore();
        await store.SaveAsync("showcase-invalid", new ShowcaseProgressState(99, isActive: true));

        var controller = CreatePersistentController(store, "showcase-invalid");
        var resumed = await controller.ResumeAsync();
        var persistedState = await store.LoadAsync("showcase-invalid");

        Assert.False(resumed);
        Assert.Null(persistedState);
        Assert.False(controller.IsActive);
    }

    [AvaloniaFact]
    public async Task ResetAsync_Should_Clear_Persisted_Progress()
    {
        var store = new InMemoryShowcaseProgressStore();
        var controller = CreatePersistentController(store, "showcase-reset");

        await controller.StartAsync();
        await controller.ResetAsync();

        var persistedState = await store.LoadAsync("showcase-reset");
        Assert.Null(persistedState);
        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
    }

    [AvaloniaFact]
    public void Skip_Should_Have_Null_CurrentStep_When_IsActive_Changes()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        ShowcaseStep? observedStep = null;
        controller.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShowcaseController.IsActive) && !controller.IsActive)
            {
                observedStep = controller.CurrentStep;
            }
        };

        controller.Skip();

        Assert.Null(observedStep);
    }

    [AvaloniaFact]
    public void Complete_Should_Have_Null_CurrentStep_When_IsActive_Changes()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        ShowcaseStep? observedStep = null;
        controller.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ShowcaseController.IsActive) && !controller.IsActive)
            {
                observedStep = controller.CurrentStep;
            }
        };

        controller.Next();

        Assert.Null(observedStep);
    }

    [AvaloniaFact]
    public void Skip_Should_Notify_CurrentIndex_And_CurrentStep()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        var notified = new List<string>();
        controller.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != null)
            {
                notified.Add(e.PropertyName);
            }
        };

        controller.Skip();

        Assert.Contains(nameof(ShowcaseController.IsActive), notified);
        Assert.Contains(nameof(ShowcaseController.CurrentIndex), notified);
        Assert.Contains(nameof(ShowcaseController.CurrentStep), notified);
        Assert.Contains(nameof(ShowcaseController.CanGoPrevious), notified);
        Assert.Contains(nameof(ShowcaseController.CanGoNext), notified);
        Assert.Contains(nameof(ShowcaseController.CurrentButtonText), notified);
    }

    [AvaloniaFact]
    public void Dispose_Should_Deactivate_Controller()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        controller.Dispose();

        Assert.False(controller.IsActive);
        Assert.Equal(-1, controller.CurrentIndex);
        Assert.Null(controller.CurrentStep);
    }

    [AvaloniaFact]
    public void Dispose_Should_Be_Safe_To_Call_Multiple_Times()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Start();

        controller.Dispose();
        controller.Dispose();

        Assert.False(controller.IsActive);
    }

    [AvaloniaFact]
    public async Task StartAsync_After_Dispose_Should_Throw_ObjectDisposedException()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        controller.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => controller.StartAsync());
    }

    [AvaloniaFact]
    public async Task NextAsync_After_Dispose_Should_Throw_ObjectDisposedException()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });

        controller.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => controller.NextAsync());
    }

    [AvaloniaFact]
    public async Task Resumed_Event_Should_Fire_On_Resume()
    {
        var store = new InMemoryShowcaseProgressStore();
        var controller = CreatePersistentController(store, "showcase-resumed-event");
        await controller.StartAsync();
        await controller.NextAsync();

        var resumedController = CreatePersistentController(store, "showcase-resumed-event");
        var resumed = false;
        resumedController.Resumed += (_, _) => resumed = true;

        await resumedController.ResumeAsync();

        Assert.True(resumed);
    }

    [AvaloniaFact]
    public void Constructor_With_Steps_Should_Populate_Collection()
    {
        var steps = new[]
        {
            new ShowcaseStep { Key = "A", Title = "First" },
            new ShowcaseStep { Key = "B", Title = "Second" },
            new ShowcaseStep { Key = "C", Title = "Third" }
        };

        var controller = new ShowcaseController(steps);

        Assert.Equal(3, controller.Steps.Count);
        Assert.Equal("A", controller.Steps[0].Key);
        Assert.Equal("B", controller.Steps[1].Key);
        Assert.Equal("C", controller.Steps[2].Key);
    }

    [AvaloniaFact]
    public void Constructor_With_Steps_Should_Still_Allow_Start()
    {
        var controller = new ShowcaseController(new[]
        {
            new ShowcaseStep { Key = "Step1" }
        });

        controller.Start();

        Assert.True(controller.IsActive);
        Assert.Equal(0, controller.CurrentIndex);
    }

    private static ShowcaseController CreatePersistentController(IShowcaseProgressStore store, string key)
    {
        var controller = new ShowcaseController
        {
            ProgressStore = store,
            PersistenceKey = key
        };

        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        return controller;
    }
}
