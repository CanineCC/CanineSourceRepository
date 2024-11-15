namespace CanineSourceRepositoryTest.BpnDiagram;

public class GivenANewFeature
{
  private readonly DraftFeatureComponentAggregate _featureComponent;
  public GivenANewFeature()
  {
    _featureComponent = new DraftFeatureComponentAggregate();
    _featureComponent.Apply(_featureComponent, new DraftFeatureCreated(Guid.Empty, Guid.CreateVersion7(), "Test diagram", "An objective", "An overview"));
  }

  //[Fact]
  //public void WhenAddingARevision_ExpectNewVersion()
  //{
  //  //ARRANGE
  //  //ACT
  //  var updated = feature.NewRevision("me");

  //  //ASSERT
  //  Assert.Equal(1, updated.Version);
  //}

  //[Fact]
  //public void WhenLoadingById_ExpectNewest()
  //{
  //  //ARRANGE
  //  var version1 = BpnFeatureRepository.Add(diagram.NewRevision("me"));
  //  var version2 = BpnFeatureRepository.Add(version1.NewRevision("me"));
  //  _ = BpnFeatureRepository.Add(version2.NewRevision("me"));

  //  //ACT
  //  var newest = BpnFeatureRepository.Load(diagram.Id);

  //  //ASSERT
  //  Assert.Equal(3, newest.Version);
  //}

}
