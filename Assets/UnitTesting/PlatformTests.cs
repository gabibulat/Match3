using NUnit.Framework;

public class PlatformTests
{
    [Test]
    public void IsWindowsPlatform_ShouldReturnTrueOnWindows()
    {
        bool isWindows = IsWindowsPlatform();
        Assert.IsTrue(isWindows, "The current platform should be Windows.");
    }

    // Helper method to check if the current platform is Windows
    private bool IsWindowsPlatform()
    {
        return UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor
            || UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsPlayer;
    }
}