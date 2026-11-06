using System.Data;
using BusinessLogicLayer.Localization;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Localization;
using Moq;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public static class TestLocalization
{
    /// <summary>
    /// Creates a real localizer for the given language over the mocked unit of work.
    /// </summary>
    public static ContentLocalizer CreateLocalizer(Mock<ISqlServerUnitOfWork> unitOfWorkMock, string languageCode = Languages.English)
    {
        return new ContentLocalizer(unitOfWorkMock.Object, Mock.Of<ICurrentLanguage>(l => l.Code == languageCode));
    }

    /// <summary>
    /// Makes the mocked unit of work run the transaction actions, as the real one does.
    /// </summary>
    public static void RunTransactions<TResult>(this Mock<ISqlServerUnitOfWork> unitOfWorkMock)
    {
        unitOfWorkMock.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task<TResult>>>(), It.IsAny<IsolationLevel>()))
            .Returns((Func<Task<TResult>> action, IsolationLevel _) => action());

        unitOfWorkMock.Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<IsolationLevel>()))
            .Returns((Func<Task> action, IsolationLevel _) => action());
    }
}
