// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using MediatR;
// using Moq;
// using Teams.APP.Layer.EventNotification;
// using Teams.CORE.Layer.CoreEvents;
// using Teams.INFRA.Layer.Dispatchers;
// using Xunit;

// namespace Teams.Tests.INFRA;

// public class DomainEventDispatcherTest
// {
//     [Fact]
//     public async Task DispatchAsync_ShouldPublishAllEvents()
//     {
//         // Arrange
//         var mediatorMock = new Mock<IMediator>();
//         var dispatcher = new DomainEventDispatcher(mediatorMock.Object);

//         var event1 = new TeamArchiveEvent(Guid.NewGuid());
//         var event2 = new TeamArchiveEvent(Guid.NewGuid());
//         var events = new List<IDomainEvent> { event1, event2 };

//         // Act
//         await dispatcher.DispatchAsync(events);

//         // Assert
//         // Vérifie que Publish a été appelé pour chaque event
//         mediatorMock.Verify(
//             m =>
//                 m.Publish(
//                     It.Is<INotification>(n =>
//                         n.GetType() == typeof(DomainEventNotification<TeamArchiveEvent>)
//                     ),
//                     It.IsAny<CancellationToken>()
//                 ),
//             Times.Exactly(2)
//         );
//     }

//     [Fact]
//     public async Task DispatchAsync_ShouldPassCancellationTokenToMediator()
//     {
//         // Arrange
//         var mediatorMock = new Mock<IMediator>();
//         var dispatcher = new DomainEventDispatcher(mediatorMock.Object);

//         var event1 = new TeamArchiveEvent(Guid.NewGuid());
//         var events = new List<IDomainEvent> { event1 };
//         var cts = new CancellationTokenSource();

//         // Act
//         await dispatcher.DispatchAsync(events, cts.Token);

//         // Assert
//         mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), cts.Token), Times.Once);
//     }
// }
