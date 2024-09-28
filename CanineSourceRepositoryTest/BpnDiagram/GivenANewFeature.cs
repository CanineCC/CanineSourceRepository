using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnDraftFeatureProjection;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;

namespace CanineSourceRepositoryTest.BpnDiagram;

public class GivenANewFeature
{
  private readonly BpnDraftFeatureAggregate feature;
  public GivenANewFeature()
  {
    feature = new BpnDraftFeatureAggregate();
    feature.Apply(feature, new BpnDraftFeature.DraftFeatureCreated(Guid.CreateVersion7(), "Test diagram", "An objective", "An overview"));
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
