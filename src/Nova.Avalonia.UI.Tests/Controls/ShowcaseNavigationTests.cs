using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShowcaseNavigationTests
{
    [AvaloniaFact]
    public void Showcase_Should_Have_Default_Values()
    {
        var showcase = new Showcase();

        Assert.Empty(showcase.Steps);
        Assert.Null(showcase.CurrentStep);
        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.False(showcase.IsActive);
        Assert.False(showcase.CanGoPrevious);
        Assert.False(showcase.CanGoNext);
    }

    [AvaloniaFact]
    public void Start_Should_Set_CurrentIndex_To_Zero()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        showcase.Start();

        Assert.Equal(0, showcase.CurrentIndex);
        Assert.True(showcase.IsActive);
        Assert.NotNull(showcase.CurrentStep);
        Assert.Equal("Step1", showcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Start_Should_Throw_If_No_Steps()
    {
        var showcase = new Showcase();

        var exception = Assert.Throws<InvalidOperationException>(showcase.Start);

        Assert.Contains("does not define any steps", exception.Message);
        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.False(showcase.IsActive);
    }

    [AvaloniaFact]
    public void TryStart_Should_Return_Validation_Result()
    {
        var showcase = new Showcase();

        var failed = showcase.TryStart();

        Assert.False(failed.Started);
        Assert.False(failed.ValidationResult.IsValid);

        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        var started = showcase.TryStart();

        Assert.True(started.Started);
        Assert.True(showcase.IsActive);
    }

    [AvaloniaFact]
    public void Start_While_Active_Should_Restart_From_Beginning()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();
        showcase.Next();

        Assert.Equal(1, showcase.CurrentIndex);

        var startedCount = 0;
        showcase.Started += (s, e) => startedCount++;

        showcase.Start();

        Assert.Equal(0, showcase.CurrentIndex);
        Assert.True(showcase.IsActive);
        Assert.Equal(1, startedCount);
    }

    [AvaloniaFact]
    public void Next_Should_Advance_To_Next_Step()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        showcase.Next();

        Assert.Equal(1, showcase.CurrentIndex);
        Assert.Equal("Step2", showcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Next_On_Last_Step_Should_Complete()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();
        showcase.Next();

        var completed = false;
        showcase.Completed += (s, e) => completed = true;

        showcase.Next();

        Assert.True(completed);
        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public void Next_When_Not_Active_Should_Do_Nothing()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        showcase.Next();

        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.False(showcase.IsActive);
    }

    [AvaloniaFact]
    public void Previous_Should_Go_Back()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();
        showcase.Next();

        showcase.Previous();

        Assert.Equal(0, showcase.CurrentIndex);
        Assert.Equal("Step1", showcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Previous_On_First_Step_Should_Do_Nothing()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Start();

        showcase.Previous();

        Assert.Equal(0, showcase.CurrentIndex);
        Assert.False(showcase.CanGoPrevious);
    }

    [AvaloniaFact]
    public void Previous_When_Not_Active_Should_Do_Nothing()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });

        showcase.Previous();

        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.False(showcase.IsActive);
    }

    [AvaloniaFact]
    public void Skip_Should_End_Showcase()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        var skipped = false;
        showcase.Skipped += (s, e) => skipped = true;

        showcase.Skip();

        Assert.True(skipped);
        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public void Skip_When_Not_Active_Should_Do_Nothing()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        var skipped = false;
        showcase.Skipped += (s, e) => skipped = true;

        showcase.Skip();

        Assert.False(skipped);
    }

    [AvaloniaFact]
    public void CanGoPrevious_Should_Be_True_After_First_Step()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        Assert.False(showcase.CanGoPrevious);

        showcase.Next();

        Assert.True(showcase.CanGoPrevious);
    }

    [AvaloniaFact]
    public void CanGoNext_Should_Be_False_On_Last_Step()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        Assert.True(showcase.CanGoNext);

        showcase.Next();

        Assert.False(showcase.CanGoNext);
    }

    [AvaloniaFact]
    public void CurrentButtonText_Should_Change_On_Last_Step()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        Assert.Equal("Next", showcase.CurrentButtonText);

        showcase.Next();

        Assert.Equal("Finish", showcase.CurrentButtonText);
    }

    [AvaloniaFact]
    public void Custom_Button_Texts_Should_Be_Used()
    {
        var showcase = new Showcase
        {
            NextButtonText = "Siguiente",
            FinishButtonText = "Finalizar",
            PreviousButtonText = "Anterior",
            SkipButtonText = "Omitir"
        };
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        Assert.Equal("Siguiente", showcase.CurrentButtonText);
        Assert.Equal("Anterior", showcase.PreviousButtonText);
        Assert.Equal("Omitir", showcase.SkipButtonText);

        showcase.Next();

        Assert.Equal("Finalizar", showcase.CurrentButtonText);
    }

    [AvaloniaFact]
    public void StepChanged_Event_Should_Fire()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });

        ShowcaseStepChangedEventArgs? args = null;
        showcase.StepChanged += (s, e) => args = e;

        showcase.Start();

        Assert.NotNull(args);
        Assert.Null(args!.PreviousStep);
        Assert.Equal("Step1", args.CurrentStep.Key);
        Assert.Equal(0, args.CurrentIndex);

        showcase.Next();

        Assert.Equal("Step1", args.PreviousStep!.Key);
        Assert.Equal("Step2", args.CurrentStep.Key);
        Assert.Equal(1, args.CurrentIndex);
    }

    [AvaloniaFact]
    public void Started_Event_Should_Fire()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        var started = false;
        showcase.Started += (s, e) => started = true;

        showcase.Start();

        Assert.True(started);
    }

    [AvaloniaFact]
    public void Reset_Should_Clear_State()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Start();

        showcase.Reset();

        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.Null(showcase.CurrentStep);
    }

    [AvaloniaFact]
    public void NextCommand_CanExecute_Should_Reflect_IsActive()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        Assert.False(showcase.NextCommand.CanExecute(null));

        showcase.Start();

        Assert.True(showcase.NextCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public void PreviousCommand_CanExecute_Should_Reflect_State()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });

        Assert.False(showcase.PreviousCommand.CanExecute(null));

        showcase.Start();
        Assert.False(showcase.PreviousCommand.CanExecute(null));

        showcase.Next();
        Assert.True(showcase.PreviousCommand.CanExecute(null));
    }

    [AvaloniaFact]
    public async Task StartAsync_Should_Invoke_Step_Hooks()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        ShowcaseStepTransitionContext? beforeContext = null;
        ShowcaseStepTransitionContext? afterContext = null;
        showcase.BeforeStepAsync = (context, _) =>
        {
            beforeContext = context;
            return Task.CompletedTask;
        };
        showcase.AfterStepAsync = (context, _) =>
        {
            afterContext = context;
            return Task.CompletedTask;
        };

        await showcase.StartAsync();

        Assert.NotNull(beforeContext);
        Assert.NotNull(afterContext);
        Assert.Equal(ShowcaseStepTransitionReason.Start, beforeContext!.Reason);
        Assert.Same(showcase, beforeContext.Showcase);
        Assert.Equal("Step1", beforeContext.NextStep.Key);
        Assert.Null(beforeContext.PreviousStep);
        Assert.Equal(0, afterContext!.NextIndex);
        Assert.Equal(0, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task NextAsync_Should_Invoke_Hooks_With_Next_Context()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        await showcase.StartAsync();

        ShowcaseStepTransitionContext? beforeContext = null;
        showcase.BeforeStepAsync = (context, _) =>
        {
            beforeContext = context;
            return Task.CompletedTask;
        };

        await showcase.NextAsync();

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
        var showcase = CreatePersistentShowcase(store, "showcase-resume");

        await showcase.StartAsync();
        await showcase.NextAsync();

        var resumedShowcase = CreatePersistentShowcase(store, "showcase-resume");
        var resumed = await resumedShowcase.ResumeAsync();

        Assert.True(resumed);
        Assert.True(resumedShowcase.IsActive);
        Assert.Equal(1, resumedShowcase.CurrentIndex);
        Assert.Equal("Step2", resumedShowcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void ProgressState_Should_RoundTrip_Through_SystemTextJson()
    {
        var state = new ShowcaseProgressState(1, isActive: true, "second");

        var json = JsonSerializer.Serialize(state);
        var restored = JsonSerializer.Deserialize<ShowcaseProgressState>(json);

        Assert.NotNull(restored);
        Assert.Equal(1, restored!.CurrentIndex);
        Assert.True(restored.IsActive);
        Assert.Equal("second", restored.StepKey);
    }

    [AvaloniaFact]
    public async Task Save_Failure_Should_Not_Commit_The_New_Step()
    {
        var store = new ThrowingShowcaseProgressStore { ThrowOnSave = true };
        var showcase = CreatePersistentShowcase(store, "showcase-save-failure");
        var stepChanged = false;
        showcase.StepChanged += (_, _) => stepChanged = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => showcase.StartAsync());

        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.Null(showcase.CurrentStep);
        Assert.False(stepChanged);
    }

    [AvaloniaFact]
    public async Task Clear_Failure_Should_Not_Deactivate_The_Showcase()
    {
        var store = new ThrowingShowcaseProgressStore();
        var showcase = CreatePersistentShowcase(store, "showcase-clear-failure");
        await showcase.StartAsync();
        store.ThrowOnClear = true;
        var skipped = false;
        showcase.Skipped += (_, _) => skipped = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => showcase.SkipAsync());

        Assert.True(showcase.IsActive);
        Assert.Equal(0, showcase.CurrentIndex);
        Assert.False(skipped);
    }

    [AvaloniaFact]
    public async Task ResumeAsync_Should_Return_False_When_No_Persisted_Progress_Exists()
    {
        var showcase = CreatePersistentShowcase(new InMemoryShowcaseProgressStore(), "missing");

        var resumed = await showcase.ResumeAsync();

        Assert.False(resumed);
        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task SkipAsync_Should_Clear_Persisted_Progress()
    {
        var store = new InMemoryShowcaseProgressStore();
        var showcase = CreatePersistentShowcase(store, "showcase-skip");

        await showcase.StartAsync();
        await showcase.SkipAsync();

        var resumedShowcase = CreatePersistentShowcase(store, "showcase-skip");
        var resumed = await resumedShowcase.ResumeAsync();

        Assert.False(resumed);
    }

    [AvaloniaFact]
    public async Task Newer_Transition_Should_Cancel_InFlight_Hook()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });

        var nextHookStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        showcase.BeforeStepAsync = (context, cancellationToken) =>
        {
            if (context.Reason != ShowcaseStepTransitionReason.Next)
                return Task.CompletedTask;

            nextHookStarted.TrySetResult();
            return Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        };

        await showcase.StartAsync();

        var nextTask = showcase.NextAsync();
        await nextHookStarted.Task;
        await showcase.SkipAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await nextTask);
        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task Completed_Save_Should_Commit_Before_A_Newer_Transition_Runs()
    {
        var store = new DelayedShowcaseProgressStore();
        var showcase = CreatePersistentShowcase(store, "showcase-save-race");
        await showcase.StartAsync();
        store.DelayNextSave = true;

        var nextTask = showcase.NextAsync();
        await store.SaveStarted;
        var previousTask = showcase.PreviousAsync();
        store.ReleaseSave();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await nextTask);
        await previousTask;

        var persistedState = await store.LoadAsync("showcase-save-race");
        Assert.NotNull(persistedState);
        Assert.Equal(0, persistedState!.CurrentIndex);
        Assert.Equal(0, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task Changing_Steps_Should_Cancel_InFlight_Navigation()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        await showcase.StartAsync();

        var hookStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseHook = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        showcase.BeforeStepAsync = async (context, _) =>
        {
            if (context.Reason == ShowcaseStepTransitionReason.Next)
            {
                hookStarted.TrySetResult();
                await releaseHook.Task;
            }
        };

        var navigation = showcase.NextAsync();
        await hookStarted.Task;
        showcase.Steps.RemoveAt(1);
        releaseHook.TrySetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await navigation);
        Assert.True(showcase.IsActive);
        Assert.Equal(0, showcase.CurrentIndex);
        Assert.Equal("Step1", showcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public async Task Sync_Start_Should_Raise_TransitionFailed_When_Hook_Throws()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.BeforeStepAsync = (_, _) => throw new InvalidOperationException("boom");

        var failure = new TaskCompletionSource<ShowcaseTransitionFailedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        showcase.TransitionFailed += (_, args) => failure.TrySetResult(args);

        showcase.Start();

        var args = await failure.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal(ShowcaseNavigationAction.Start, args.Action);
        Assert.IsType<InvalidOperationException>(args.Exception);
        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task ResumeAsync_Should_Clear_Invalid_Persisted_Index()
    {
        var store = new InMemoryShowcaseProgressStore();
        await store.SaveAsync("showcase-invalid", new ShowcaseProgressState(99, isActive: true));

        var showcase = CreatePersistentShowcase(store, "showcase-invalid");
        var resumed = await showcase.ResumeAsync();
        var persistedState = await store.LoadAsync("showcase-invalid");

        Assert.False(resumed);
        Assert.Null(persistedState);
        Assert.False(showcase.IsActive);
    }

    [AvaloniaFact]
    public async Task ResetAsync_Should_Clear_Persisted_Progress()
    {
        var store = new InMemoryShowcaseProgressStore();
        var showcase = CreatePersistentShowcase(store, "showcase-reset");

        await showcase.StartAsync();
        await showcase.ResetAsync();

        var persistedState = await store.LoadAsync("showcase-reset");
        Assert.Null(persistedState);
        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public void Skip_Should_Have_Null_CurrentStep_When_IsActive_Changes()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Start();

        ShowcaseStep? observedStep = null;
        showcase.PropertyChanged += (_, e) =>
        {
            if (e.Property == Showcase.IsActiveProperty && !showcase.IsActive)
            {
                observedStep = showcase.CurrentStep;
            }
        };

        showcase.Skip();

        Assert.Null(observedStep);
    }

    [AvaloniaFact]
    public void Complete_Should_Have_Null_CurrentStep_When_IsActive_Changes()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Start();

        ShowcaseStep? observedStep = null;
        showcase.PropertyChanged += (_, e) =>
        {
            if (e.Property == Showcase.IsActiveProperty && !showcase.IsActive)
            {
                observedStep = showcase.CurrentStep;
            }
        };

        showcase.Next();

        Assert.Null(observedStep);
    }

    [AvaloniaFact]
    public void Skip_Should_Notify_CurrentIndex_And_CurrentStep()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();
        showcase.Next();

        var notified = new List<string>();
        showcase.PropertyChanged += (_, e) =>
        {
            notified.Add(e.Property.Name);
        };

        showcase.Skip();

        Assert.Contains(nameof(Showcase.IsActive), notified);
        Assert.Contains(nameof(Showcase.CurrentIndex), notified);
        Assert.Contains(nameof(Showcase.CurrentStep), notified);
        Assert.Contains(nameof(Showcase.CanGoPrevious), notified);
    }

    [AvaloniaFact]
    public void Clearing_Steps_Should_Deactivate_Showcase()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Start();

        showcase.Steps.Clear();

        Assert.False(showcase.IsActive);
        Assert.Equal(-1, showcase.CurrentIndex);
        Assert.Null(showcase.CurrentStep);
    }

    [AvaloniaFact]
    public void Removing_Current_Step_Should_Keep_A_Valid_Active_Step()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        showcase.Start();

        showcase.Steps.RemoveAt(0);

        Assert.True(showcase.IsActive);
        Assert.Equal(0, showcase.CurrentIndex);
        Assert.Equal("Step2", showcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public void Moving_Steps_Should_Preserve_The_Current_Step()
    {
        var showcase = new Showcase();
        var first = new ShowcaseStep { Key = "Step1" };
        var second = new ShowcaseStep { Key = "Step2" };
        showcase.Steps.Add(first);
        showcase.Steps.Add(second);
        showcase.Start();
        showcase.Next();

        showcase.Steps.Move(1, 0);

        Assert.Same(second, showcase.CurrentStep);
        Assert.Equal(0, showcase.CurrentIndex);
    }

    [AvaloniaFact]
    public async Task Moving_Steps_Should_Persist_The_Current_Step_Key()
    {
        var store = new InMemoryShowcaseProgressStore();
        var showcase = CreatePersistentShowcase(store, "showcase-move");
        await showcase.StartAsync();
        await showcase.NextAsync();

        showcase.Steps.Move(1, 0);

        var persisted = await store.LoadAsync("showcase-move");
        Assert.NotNull(persisted);
        Assert.Equal(0, persisted!.CurrentIndex);
        Assert.Equal("Step2", persisted.StepKey);

        var resumedShowcase = CreatePersistentShowcase(store, "showcase-move");
        resumedShowcase.Steps.Move(1, 0);

        Assert.True(await resumedShowcase.ResumeAsync());
        Assert.Equal(0, resumedShowcase.CurrentIndex);
        Assert.Equal("Step2", resumedShowcase.CurrentStep!.Key);
    }

    [AvaloniaFact]
    public async Task Step_Id_Should_Resume_Repeated_Targets_After_Reordering()
    {
        var store = new InMemoryShowcaseProgressStore();
        var showcase = new Showcase
        {
            ProgressStore = store,
            PersistenceKey = "showcase-repeated-target"
        };
        showcase.Steps.Add(new ShowcaseStep { Id = "overview", Key = "Shared" });
        showcase.Steps.Add(new ShowcaseStep { Id = "actions", Key = "Shared" });
        await showcase.StartAsync();
        await showcase.NextAsync();

        var resumedShowcase = new Showcase
        {
            ProgressStore = store,
            PersistenceKey = "showcase-repeated-target"
        };
        resumedShowcase.Steps.Add(new ShowcaseStep { Id = "actions", Key = "Shared" });
        resumedShowcase.Steps.Add(new ShowcaseStep { Id = "overview", Key = "Shared" });

        Assert.True(await resumedShowcase.ResumeAsync());
        Assert.Equal(0, resumedShowcase.CurrentIndex);
        Assert.Equal("actions", resumedShowcase.CurrentStep!.Id);
    }

    [AvaloniaFact]
    public void Repeated_Targets_Without_Ids_Should_Report_A_Persistence_Warning()
    {
        var showcase = new Showcase
        {
            ProgressStore = new InMemoryShowcaseProgressStore(),
            PersistenceKey = "showcase-ambiguous"
        };
        showcase.Steps.Add(new ShowcaseStep { Key = "Shared" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Shared" });

        var validation = showcase.Validate();

        Assert.True(validation.IsValid);
        Assert.Contains(
            validation.Issues,
            issue => issue.Code == ShowcaseValidationIssueCode.AmbiguousStepIdentity);
    }

    [AvaloniaFact]
    public async Task Resumed_Event_Should_Fire_On_Resume()
    {
        var store = new InMemoryShowcaseProgressStore();
        var showcase = CreatePersistentShowcase(store, "showcase-resumed-event");
        await showcase.StartAsync();
        await showcase.NextAsync();

        var resumedShowcase = CreatePersistentShowcase(store, "showcase-resumed-event");
        var resumed = false;
        resumedShowcase.Resumed += (_, _) => resumed = true;

        await resumedShowcase.ResumeAsync();

        Assert.True(resumed);
    }

    [AvaloniaFact]
    public void Steps_Should_Accept_An_Existing_Sequence()
    {
        var steps = new[]
        {
            new ShowcaseStep { Key = "A", Title = "First" },
            new ShowcaseStep { Key = "B", Title = "Second" },
            new ShowcaseStep { Key = "C", Title = "Third" }
        };

        var showcase = new Showcase();
        foreach (var step in steps)
        {
            showcase.Steps.Add(step);
        }

        Assert.Equal(3, showcase.Steps.Count);
        Assert.Equal("A", showcase.Steps[0].Key);
        Assert.Equal("B", showcase.Steps[1].Key);
        Assert.Equal("C", showcase.Steps[2].Key);
    }

    [AvaloniaFact]
    public void Populated_Steps_Should_Allow_Start()
    {
        var showcase = new Showcase();
        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });

        showcase.Start();

        Assert.True(showcase.IsActive);
        Assert.Equal(0, showcase.CurrentIndex);
    }

    private static Showcase CreatePersistentShowcase(IShowcaseProgressStore store, string key)
    {
        var showcase = new Showcase
        {
            ProgressStore = store,
            PersistenceKey = key
        };

        showcase.Steps.Add(new ShowcaseStep { Key = "Step1" });
        showcase.Steps.Add(new ShowcaseStep { Key = "Step2" });
        return showcase;
    }
}
