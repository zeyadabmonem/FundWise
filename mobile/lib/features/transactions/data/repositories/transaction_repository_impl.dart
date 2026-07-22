import '../../../auth/domain/entities/auth_user.dart';
import '../../../transactions/presentation/bloc/transaction_bloc.dart';
import '../../domain/repositories/transaction_repository.dart';
import '../datasources/transaction_remote_datasource.dart';
import 'package:dartz/dartz.dart';

class TransactionRepositoryImpl implements TransactionRepository {
  final TransactionRemoteDatasource _remote;
  TransactionRepositoryImpl(this._remote);

  @override
  Future<Either<Failure, List<Transaction>>> getTransactions({int? month, int? year}) async {
    try {
      final list = await _remote.getTransactions(month: month, year: year);
      return Right(list);
    } catch (e) {
      return Left(const Failure(message: 'Failed to load transactions.'));
    }
  }

  @override
  Future<Either<Failure, Transaction>> createTransaction({
    required String merchant,
    required double amount,
    String? category,
    DateTime? captureDate,
    String? notes,
  }) async {
    try {
      final t = await _remote.createTransaction(
        merchant: merchant,
        amount: amount,
        category: category,
        captureDate: captureDate,
        notes: notes,
      );
      return Right(t);
    } catch (e) {
      return Left(const Failure(message: 'Failed to create transaction.'));
    }
  }
}
