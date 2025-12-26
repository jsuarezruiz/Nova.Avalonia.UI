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
    public void NextButtonText_Should_Change_On_Last_Step()
    {
        var controller = new ShowcaseController();
        controller.Steps.Add(new ShowcaseStep { Key = "Step1" });
        controller.Steps.Add(new ShowcaseStep { Key = "Step2" });
        controller.Start();
        
        Assert.Equal("Next", controller.NextButtonText);
        
        controller.Next();
        
        Assert.Equal("Finish", controller.NextButtonText);
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
}
