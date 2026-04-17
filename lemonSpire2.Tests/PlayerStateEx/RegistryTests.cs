using Godot;
using lemonSpire2.PlayerStateEx;
using lemonSpire2.PlayerStateEx.PanelProvider;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using Xunit;

namespace lemonSpire2.Tests.PlayerStateEx;

public sealed class RegistryTests
{
    [Fact]
    public void PlayerTooltipRegistry_Register_ShouldThrow_WhenIdAlreadyExists()
    {
        PlayerTooltipRegistry.Clear();

        try
        {
            PlayerTooltipRegistry.Register(new FakeTooltipProvider("dup"));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                PlayerTooltipRegistry.Register(new FakeTooltipProvider("dup")));

            Assert.Contains("dup", ex.Message);
        }
        finally
        {
            PlayerTooltipRegistry.Clear();
        }
    }

    [Fact]
    public void PlayerPanelRegistry_Register_ShouldThrow_WhenIdAlreadyExists()
    {
        PlayerPanelRegistry.Clear();

        try
        {
            PlayerPanelRegistry.Register(new FakePanelProvider("dup"));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                PlayerPanelRegistry.Register(new FakePanelProvider("dup")));

            Assert.Contains("dup", ex.Message);
        }
        finally
        {
            PlayerPanelRegistry.Clear();
        }
    }

    private sealed class FakeTooltipProvider(string id) : ITooltipProvider
    {
        public string Id => id;

        public HoverTip? CreateHoverTip(Player player)
        {
            return default;
        }
    }

    private sealed class FakePanelProvider(string id) : IPlayerPanelProvider
    {
        public string Id => id;
        public int Priority => 100;
        public string DisplayName => id;

        public bool ShouldShow(Player player)
        {
            return true;
        }

        public Control CreateContent(Player player)
        {
            return new Control();
        }

        public void UpdateContent(Player player, Control content)
        {
        }

        public Action? SubscribeEvents(Player player, Action onUpdate)
        {
            return null;
        }

        public void Cleanup(Control content)
        {
        }
    }
}
