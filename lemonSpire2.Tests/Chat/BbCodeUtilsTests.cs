using lemonSpire2.util;
using Xunit;

namespace lemonSpire2.Tests.Chat;

public sealed class BbCodeUtilsTests
{
    [Fact]
    public void AutoCloseUnclosedTags_ShouldTreatSquareBracketInlineReferenceAsBbCode()
    {
        var result = BbCodeUtils.AutoCloseUnclosedTags("[card:CLASH]");

        Assert.Equal("[card:CLASH][/card:CLASH]", result);
    }
}
