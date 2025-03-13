using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using System.IO;
using System.Net.Sockets;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.EFS;

namespace Infrastructure;

class VueAppStack : NestedStack
{
    public VueAppStack(Stack stack, string id, NestedStackProps props)
        : base(stack, id, props)
    {
        var domainName = "mkscoreapp.geldhof.eu";

        var bucket = new Bucket(
            this,
            "MkScoreAppBucket",
            new BucketProps
            {
                BucketName = domainName,
                AccessControl = BucketAccessControl.PRIVATE,
                RemovalPolicy = RemovalPolicy.DESTROY,
                AutoDeleteObjects = true,
            });

        _ = new CfnOutput(this, "MkScoreAppBucketUrl", new CfnOutputProps { Value = bucket.BucketWebsiteUrl });

        _ = new BucketDeployment(this, "MkScoreAppBucketDeployment", new BucketDeploymentProps
        {
            DestinationBucket = bucket,
            Sources = [Source.Asset("src/mkscoreapp/dist")]
        });

        var certificateArn = "arn:aws:acm:us-east-1:766704866489:certificate/9b5cfec9-1980-4b3c-8e9f-488f84c00b98";
        var certificate = Certificate.FromCertificateArn(this, "MkScoreStaticSiteCertificate", certificateArn);

        var cdn = new Distribution(this, "MkScoreAppDistribution", new DistributionProps
        {
            DomainNames = [domainName],
            Certificate = certificate,
            DefaultRootObject = "index.html",
            DefaultBehavior = new BehaviorOptions
            {
                Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
            },
        });

        _ = new CfnOutput(this, "MkScoreAppDistributionDomainName", new CfnOutputProps { Value = cdn.DistributionDomainName });

    }
}
