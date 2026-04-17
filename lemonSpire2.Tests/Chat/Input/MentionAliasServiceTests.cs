using lemonSpire2.Chat.Input.Service.Mention;
using Xunit;

namespace lemonSpire2.Tests.Chat.Input;

public sealed class MentionAliasServiceTests
{
    [Fact]
    public void CreateAliases_ShouldSuffixDuplicateDisplayNames_ByNetIdOrder()
    {
        var aliases = MentionAliasService.CreateAliases(
        [
            new MentionAliasSource("Alice", 5),
            new MentionAliasSource("Bob", 1),
            new MentionAliasSource("Alice", 2),
            new MentionAliasSource("Alice", 3)
        ]);

        Assert.Equal("Alice_1", aliases[2]);
        Assert.Equal("Alice_2", aliases[3]);
        Assert.Equal("Alice_3", aliases[5]);
        Assert.Equal("Bob", aliases[1]);
    }
}
