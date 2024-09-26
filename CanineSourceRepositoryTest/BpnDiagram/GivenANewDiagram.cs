using CanineSourceRepository.BusinessProcessNotation.Context.Feature;

namespace CanineSourceRepositoryTest.BpnDiagram;

public class GivenANewDiagram
{
  private readonly BpnFeature diagram;
  public GivenANewDiagram()
  {
    diagram = BpnFeature.CreateNew("Test diagram", [], [], [BpnFeature.Environment.Development, BpnFeature.Environment.Testing]);
  }

  [Fact]
  public void WhenAddingARevision_ExpectNewVersion()
  {
    //ARRANGE
    //ACT
    var updated = diagram.NewRevision("me");

    //ASSERT
    Assert.Equal(1, updated.Version);
  }

  [Fact]
  public void WhenLoadingById_ExpectNewest()
  {
    //ARRANGE
    var version1 = BpnFeatureRepository.Add(diagram.NewRevision("me"));
    var version2 = BpnFeatureRepository.Add(version1.NewRevision("me"));
    _ = BpnFeatureRepository.Add(version2.NewRevision("me"));

    //ACT
    var newest = BpnFeatureRepository.Load(diagram.Id);

    //ASSERT
    Assert.Equal(3, newest.Version);
  }

}
