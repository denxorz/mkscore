using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.DynamoDB;

namespace Infrastructure;

class VueAppStack
{
    public VueAppStack(Stack stack, string id, VueAppStackProps props)
    {
        var domainName = $"mkscore{(props.IsDev ? "-dev" : "")}.geldhof.eu";

        var bucket = new Bucket(
            stack,
            id + "Bucket",
            new BucketProps
            {
                BucketName = domainName,
                AccessControl = BucketAccessControl.PRIVATE,
                RemovalPolicy = RemovalPolicy.DESTROY,
                AutoDeleteObjects = true,
            });

        _ = new CfnOutput(stack, id + "BucketUrl", new CfnOutputProps { Value = bucket.BucketWebsiteUrl });

        _ = new BucketDeployment(stack, id + "BucketDeployment", new BucketDeploymentProps
        {
            DestinationBucket = bucket,
            Sources = [Source.Asset("src/mkscoreapp/dist")]
        });

        var cdn = new Distribution(
            stack,
            id + "Distribution",
            new DistributionProps
            {
                DomainNames = [domainName],
                Certificate = props.Certificate,
                DefaultRootObject = "index.html",
                DefaultBehavior = new BehaviorOptions
                {
                    Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
                },
            });

        _ = new CfnOutput(stack, id + "DomainName", new CfnOutputProps { Value = cdn.DistributionDomainName });
    }

    public class VueAppStackProps : StackProps
    {
        public bool IsDev { get; internal set; }
        public ICertificate Certificate { get; internal set; }
    }
}
