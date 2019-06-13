﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder.Generators;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Orckestra.Composer.Parameters;
using Orckestra.Composer.Product.Providers;
using Orckestra.Composer.Repositories;
using Orckestra.Overture.ServiceModel.Orders;

namespace Orckestra.Composer.Product.Tests.Providers
{
    [TestFixture]
    public class ConfInventoryLocationProvider_GetFulfillmentLocationAsync
    {
        public Guid ValidLocationId { get; set; }

        public AutoMocker Container { get; set; }

        [SetUp]
        public void SetUp()
        {
            Container = new AutoMocker();

            ComposerConfiguration.DefaultInventoryLocationId = GetRandom.String(6);
            ValidLocationId = GetRandom.Guid();

            var repoMock = Container.GetMock<IFulfillmentLocationsRepository>();
            repoMock.Setup(
                repo => repo.GetFulfillmentLocationsByScopeAsync(It.IsNotNull<GetFulfillmentLocationsByScopeParam>()))
                .ReturnsAsync(new List<FulfillmentLocation>
                {
                    new FulfillmentLocation
                    {
                        Id = GetRandom.Guid(),
                        IsActive = false,
                        InventoryLocationId = ComposerConfiguration.DefaultInventoryLocationId
                    },
                    new FulfillmentLocation
                    {
                        Id = GetRandom.Guid(),
                        IsActive = true,
                        InventoryLocationId = GetRandom.String(6)
                    },
                    new FulfillmentLocation
                    {
                        Id = ValidLocationId,
                        IsActive = true,
                        InventoryLocationId = ComposerConfiguration.DefaultInventoryLocationId
                    }
                });
        }

        [Test]
        public async Task WHEN_method_is_invoked_SHOULD_call_repo_for_fulfillment_locations()
        {
            //Arrange
            var p = new GetFulfillmentLocationParam
            {
                Scope = GetRandom.String(6)
            };

            var sut = Container.CreateInstance<ConfigurationInventoryLocationProvider>();

            //Act
            await sut.GetFulfillmentLocationAsync(p);

            //Assert
            Container.Verify<IFulfillmentLocationsRepository>(repo => repo.GetFulfillmentLocationsByScopeAsync(It.IsNotNull<GetFulfillmentLocationsByScopeParam>()));
        }

        [Test]
        public async Task WHEN_method_is_invoked_SHOULD_return_first_valid_fulfillment_location_id()
        {
            //Arrange
            var p = new GetFulfillmentLocationParam
            {
                Scope = GetRandom.String(7)
            };

            var sut = Container.CreateInstance<ConfigurationInventoryLocationProvider>();

            //Act
            var location = await sut.GetFulfillmentLocationAsync(p);

            //Assert
            location.Should().NotBeNull();
            location.Id.Should().Be(ValidLocationId);
        }

        [Test]
        public void WHEN_Locations_do_not_contain_default_id_THROWS_ArgumentException()
        {
            //Arrange
            ComposerConfiguration.DefaultInventoryLocationId = GetRandom.String(6); //Changing the ID between generation of list and execution of SUT.
            var p = new GetFulfillmentLocationParam
            {
                Scope = GetRandom.String(7)
            };

            var sut = Container.CreateInstance<ConfigurationInventoryLocationProvider>();

            //Act
            var exception = Assert.Throws<ArgumentException>(async () => await sut.GetFulfillmentLocationAsync(p));

            //Assert
            exception.Should().NotBeNull();
            exception.ParamName.ShouldBeEquivalentTo("param");
            exception.Message.Should().ContainEquivalentOf(p.Scope);
            exception.Message.Should().ContainEquivalentOf(ComposerConfiguration.DefaultInventoryLocationId);
        }

    }
}