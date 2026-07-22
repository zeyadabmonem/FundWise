import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../domain/usecases/create_transaction_usecase.dart';
import '../../domain/usecases/get_transactions_usecase.dart';

// ─── Entities ────────────────────────────────────────────────────────────────
class Transaction extends Equatable {
  final String id;
  final String merchant;
  final double amount;
  final String currency;
  final String category;
  final String categoryName;
  final String source;
  final String sourceName;
  final DateTime captureDate;
  final String? notes;
  final bool isConfirmed;
  final bool needsReview;
  final double confidenceScore;

  const Transaction({
    required this.id,
    required this.merchant,
    required this.amount,
    required this.currency,
    required this.category,
    required this.categoryName,
    required this.source,
    required this.sourceName,
    required this.captureDate,
    this.notes,
    required this.isConfirmed,
    required this.needsReview,
    required this.confidenceScore,
  });

  @override
  List<Object?> get props => [id, merchant, amount, captureDate];
}

class CaptureConfirmation extends Equatable {
  final String merchant;
  final double amount;
  final String currency;
  final String category;
  final String categoryName;
  final DateTime captureDate;
  final double confidenceScore;
  final String captureSource;
  final String rawInput;

  const CaptureConfirmation({
    required this.merchant,
    required this.amount,
    required this.currency,
    required this.category,
    required this.categoryName,
    required this.captureDate,
    required this.confidenceScore,
    required this.captureSource,
    required this.rawInput,
  });

  @override
  List<Object?> get props => [merchant, amount, captureDate];
}

// ─── Events ──────────────────────────────────────────────────────────────────
sealed class TransactionEvent extends Equatable {
  @override List<Object?> get props => [];
}

class TransactionLoadRequested extends TransactionEvent {
  final int? month;
  final int? year;
  TransactionLoadRequested({this.month, this.year});
  @override List<Object?> get props => [month, year];
}

class TransactionCreateRequested extends TransactionEvent {
  final String merchant;
  final double amount;
  final String? category;
  final DateTime? captureDate;
  final String? notes;
  TransactionCreateRequested({
    required this.merchant,
    required this.amount,
    this.category,
    this.captureDate,
    this.notes,
  });
  @override List<Object?> get props => [merchant, amount];
}

class TransactionConfirmRequested extends TransactionEvent {
  final String transactionId;
  TransactionConfirmRequested(this.transactionId);
  @override List<Object?> get props => [transactionId];
}

// ─── States ──────────────────────────────────────────────────────────────────
sealed class TransactionState extends Equatable {
  @override List<Object?> get props => [];
}

class TransactionInitial extends TransactionState {}
class TransactionLoading extends TransactionState {}
class TransactionLoaded extends TransactionState {
  final List<Transaction> transactions;
  TransactionLoaded(this.transactions);
  @override List<Object?> get props => [transactions];
}
class TransactionCreating extends TransactionState {}
class TransactionCreated extends TransactionState {
  final Transaction transaction;
  TransactionCreated(this.transaction);
  @override List<Object?> get props => [transaction];
}
class TransactionError extends TransactionState {
  final String message;
  TransactionError(this.message);
  @override List<Object?> get props => [message];
}

// ─── BLoC ─────────────────────────────────────────────────────────────────────
class TransactionBloc extends Bloc<TransactionEvent, TransactionState> {
  final GetTransactionsUsecase getTransactions;
  final CreateTransactionUsecase createTransaction;

  TransactionBloc({
    required this.getTransactions,
    required this.createTransaction,
  }) : super(TransactionInitial()) {
    on<TransactionLoadRequested>(_onLoadRequested);
    on<TransactionCreateRequested>(_onCreateRequested);
  }

  Future<void> _onLoadRequested(TransactionLoadRequested event, Emitter<TransactionState> emit) async {
    emit(TransactionLoading());
    final result = await getTransactions(TransactionParams(month: event.month, year: event.year));
    result.fold(
      (failure) => emit(TransactionError(failure.message)),
      (transactions) => emit(TransactionLoaded(transactions)),
    );
  }

  Future<void> _onCreateRequested(TransactionCreateRequested event, Emitter<TransactionState> emit) async {
    emit(TransactionCreating());
    final result = await createTransaction(CreateTransactionParams(
      merchant: event.merchant,
      amount: event.amount,
      category: event.category,
      captureDate: event.captureDate,
      notes: event.notes,
    ));
    result.fold(
      (failure) => emit(TransactionError(failure.message)),
      (transaction) => emit(TransactionCreated(transaction)),
    );
  }
}
