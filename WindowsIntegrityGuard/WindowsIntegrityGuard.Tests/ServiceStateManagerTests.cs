using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests;

public class ServiceStateManagerTests
{
    [Fact]
    public void InitialStatus_ShouldBeIdle()
    {
        var manager = new ServiceStateManager();

        Assert.Equal(ServiceStatus.Idle, manager.Status);
    }

    [Fact]
    public void StartScanning_ShouldChangeStatus()
    {
        var manager = new ServiceStateManager();

        bool result = manager.TryTransition(ServiceStatus.Scanning);

        Assert.True(result);
        Assert.Equal(ServiceStatus.Scanning, manager.Status);
    }

    [Fact]
    public void InvalidTransition_ShouldBeRejected()
    {
        var manager = new ServiceStateManager();

        bool result = manager.TryTransition(ServiceStatus.Repairing);

        Assert.False(result);
        Assert.Equal(ServiceStatus.Idle, manager.Status);
    }

    [Fact]
    public void CompletedScan_ShouldReturnToIdle()
    {
        var manager = new ServiceStateManager();

        Assert.True(manager.TryTransition(ServiceStatus.Scanning));
        Assert.True(manager.TryTransition(ServiceStatus.Completed));
        Assert.True(manager.TryTransition(ServiceStatus.Idle));

        Assert.Equal(ServiceStatus.Idle, manager.Status);
    }

    [Fact]
    public void RepairFlow_ShouldCompleteSuccessfully()
    {
        var manager = new ServiceStateManager();

        Assert.True(manager.TryTransition(ServiceStatus.Scanning));
        Assert.True(manager.TryTransition(ServiceStatus.Repairing));
        Assert.True(manager.TryTransition(ServiceStatus.Completed));

        Assert.Equal(ServiceStatus.Completed, manager.Status);
    }

    [Fact]
    public void FailedOperation_ShouldAllowReset()
    {
        var manager = new ServiceStateManager();

        Assert.True(manager.TryTransition(ServiceStatus.Scanning));
        Assert.True(manager.TryTransition(ServiceStatus.Failed));
        Assert.True(manager.TryTransition(ServiceStatus.Idle));

        Assert.Equal(ServiceStatus.Idle, manager.Status);
    }
}