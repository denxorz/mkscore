using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.CertificateManager;

namespace Infrastructure;

class VueAppStack
{
    public VueAppStack(Stack stack, string id, NestedStackProps props)
    {
        var domainName = "mkscoreapp.geldhof.eu";

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

        var certificateArn = "arn:aws:acm:us-east-1:586794442045:certificate/6658610a-6330-49c5-9c10-3a176f9dcc97";
        var certificate = Certificate.FromCertificateArn(stack, id + "Certificate", certificateArn);
        //var certificate = new Certificate(stack, id+"Certificate", new CertificateProps { DomainName = "mkscoreapp.geldhof.eu" }); //  Certificate.FromCertificateArn(stack, "Certificate", certificateArn);

        var cdn = new Distribution(
            stack,
            id + "Distribution",
            new DistributionProps
            {
                DomainNames = [domainName],
                Certificate = certificate,
                DefaultRootObject = "index.html",
                DefaultBehavior = new BehaviorOptions
                {
                    Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
                },
            });

        _ = new CfnOutput(stack, id + "DomainName", new CfnOutputProps { Value = cdn.DistributionDomainName });
    }
}
