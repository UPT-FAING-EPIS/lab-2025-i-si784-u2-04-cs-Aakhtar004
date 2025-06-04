using EcommerceApp.Api.Controllers;
using EcommerceApp.Api.Models;
using EcommerceApp.Api.Services;
using Moq;

namespace EcommerceApp.Tests;
public class ControllerTests
{
      private CartController controller;
      private Mock<IPaymentService> paymentServiceMock;
      private Mock<ICartService> cartServiceMock;
      private Mock<IShipmentService> shipmentServiceMock;
      private Mock<IDiscountService> discountServiceMock;
      private Mock<ICard> cardMock;
      private Mock<IAddressInfo> addressInfoMock;
      private List<ICartItem> items;
      private const double INITIAL_TOTAL = 100.0;
      private const double DISCOUNT_AMOUNT = 10.0;

      [SetUp]
      public void Setup()
      {
          
          cartServiceMock = new Mock<ICartService>();
          paymentServiceMock = new Mock<IPaymentService>();
          shipmentServiceMock = new Mock<IShipmentService>();
          discountServiceMock = new Mock<IDiscountService>();

          // arrange
          cardMock = new Mock<ICard>();
          addressInfoMock = new Mock<IAddressInfo>();

          // 
          var cartItemMock = new Mock<ICartItem>();
          cartItemMock.Setup(item => item.Price).Returns(10);

          items = new List<ICartItem>()
          {
              cartItemMock.Object
          };

          cartServiceMock.Setup(c => c.Items()).Returns(items.AsEnumerable());
          cartServiceMock.Setup(c => c.Total()).Returns(INITIAL_TOTAL);
          discountServiceMock.Setup(d => d.CalculateDiscount(INITIAL_TOTAL)).Returns(DISCOUNT_AMOUNT);
      }

      [Test]
      public void ShouldReturnCharged()
      {
          string expected = "charged";
          paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(true);

          // Create controller without discount service
          controller = new CartController(cartServiceMock.Object, paymentServiceMock.Object, shipmentServiceMock.Object);

          // act
          var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

          // assert
          shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());
          paymentServiceMock.Verify(p => p.Charge(INITIAL_TOTAL, cardMock.Object), Times.Once());
          Assert.That(expected, Is.EqualTo(result));
      }

      [Test]
      public void ShouldReturnNotCharged() 
      {
          string expected = "not charged";
          paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(false);

          // Create controller without discount service
          controller = new CartController(cartServiceMock.Object, paymentServiceMock.Object, shipmentServiceMock.Object);

          // act
          var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

          // assert
          shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
          Assert.That(expected, Is.EqualTo(result));
      }
      
      [TestCase(true, "charged")]
      [TestCase(false, "not charged")]
      public void ShouldApplyDiscountAndReturnCorrectStatus(bool paymentSuccess, string expected)
      {
          // arrange
          paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(paymentSuccess);
          
          // Create controller with discount service
          controller = new CartController(
              cartServiceMock.Object, 
              paymentServiceMock.Object, 
              shipmentServiceMock.Object,
              discountServiceMock.Object);

          // act
          var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

          // assert
          // Verify the discounted total was used (100 - 10 = 90)
          paymentServiceMock.Verify(p => p.Charge(INITIAL_TOTAL - DISCOUNT_AMOUNT, cardMock.Object), Times.Once());
          
          if (paymentSuccess)
          {
              shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());
          }
          else
          {
              shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
          }
          
          Assert.That(result, Is.EqualTo(expected));
      }
}