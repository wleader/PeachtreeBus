using System;

namespace PeachtreeBus.Data;

public static class BusDataAccessExtensions
{
    extension(IBusDataAccess dataAccess)
    {
        public void RollbackToSavePointAfterException(string savepointName, Exception originalException)
        {
            try
            {
                dataAccess.RollbackToSavepoint(savepointName);
            }
            catch (Exception rollbackException)
            {
                throw new AggregateException(
                    $"Unable to rollback to the savepoint '{savepointName}' after an exception. This may be because the original exception has left the database transaction in an unusable state.",
                    originalException,
                    rollbackException);
            }
        }

        public void RollbackTransactionAfterException(Exception originalException)
        {
            try
            {
                dataAccess.RollbackTransaction();
            }
            catch (Exception rollbackException)
            {
                throw new AggregateException(
                    "Unable to rollback to the after an exception. This may be because the original exception has left the database transaction in an unusable state.",
                    originalException,
                    rollbackException);
            }
        }
    }
}
