using Xunit.Isolation;
using Xunit.Isolation.SkippableFact;

[assembly: TestFramework("Xunit.Isolation.IsolationFramework", "Xunit.Isolation")]
[assembly: EnsureReferenced(typeof(SkippableFactApplyContextMethods))]
