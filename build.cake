var target = Argument("target", "Publish");

Task("Publish")
    .Does(() => 
    {
        Information("Trying to publish application");

        DotNetPublish("./", new DotNetPublishSettings 
        {
            Configuration = "Release"
        });

        Information("Publish done");
    });

 RunTarget(target);

