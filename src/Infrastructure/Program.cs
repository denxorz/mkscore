using Amazon.CDK;

namespace Infrastructure;

internal sealed class Program
{
    private Program()
    {
    }

    public static void Main(string[] args)
    {
        var app = new App();

        _ = new MkScoreStack(
            app,
            "MkScoreDev",
            new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "586794442045",
                    Region = "eu-west-1"
                }
            });

        _ = new MkScoreStack(
            app,
            "MkScoreProd",
            new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "545009864160",
                    Region = "eu-west-1"
                }
            });

        app.Synth();
    }
}
