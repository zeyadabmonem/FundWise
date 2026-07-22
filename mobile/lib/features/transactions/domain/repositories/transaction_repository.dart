import 'package:dartz/dartz.dart';
import '../../../auth/domain/entities/auth_user.dart';
import '../../../transactions/presentation/bloc/transaction_bloc.dart';

abstract class TransactionRepository {
  Future<Either<Failure, List<Transaction>>> getTransactions({int? month, int? year});
  Future<Either<Failure, Transaction>> createTransaction({
    required String merchant,
    required double amount,
    String? category,
    DateTime? captureDate,
    String? notes,
  });
}

class TransactionParams {
  final int? month;
  final int? year;
  TransactionParams({this.month, this.year});
}

class CreateTransactionParams {
  final String merchant;
  final double amount;
  final String? category;
  final DateTime? captureDate;
  final String? notes;
  CreateTransactionParams({
    required this.merchant,
    required this.amount,
    this.category,
    this.captureDate,
    this.notes,
  });
}

class GetTransactionsUsecase {
  final TransactionRepository _repository;
  GetTransactionsUsecase(this._repository);
  Future<Either<Failure, List<Transaction>>> call(TransactionParams params) =>
      _repository.getTransactions(month: params.month, year: params.year);
}

class CreateTransactionUsecase {
  final TransactionRepository _repository;
  CreateTransactionUsecase(this._repository);
  Future<Either<Failure, Transaction>> call(CreateTransactionParams params) =>
      _repository.createTransaction(
        merchant: params.merchant,
        amount: params.amount,
        category: params.category,
        captureDate: params.captureDate,
        notes: params.notes,
      );
}
