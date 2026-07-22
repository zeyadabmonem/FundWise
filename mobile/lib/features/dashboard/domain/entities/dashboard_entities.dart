import 'package:equatable/equatable.dart';

class DashboardSummary extends Equatable {
  final double totalSpent;
  final String currency;
  final int transactionCount;
  final int month;
  final int year;
  final List<CategoryBreakdown> categoryBreakdown;
  final List<RecentTransaction> recentTransactions;

  const DashboardSummary({
    required this.totalSpent,
    required this.currency,
    required this.transactionCount,
    required this.month,
    required this.year,
    required this.categoryBreakdown,
    required this.recentTransactions,
  });

  @override
  List<Object?> get props => [totalSpent, currency, transactionCount, month, year];
}

class CategoryBreakdown extends Equatable {
  final String category;
  final String categoryName;
  final double total;
  final int count;
  final double percentage;

  const CategoryBreakdown({
    required this.category,
    required this.categoryName,
    required this.total,
    required this.count,
    required this.percentage,
  });

  @override
  List<Object?> get props => [category, total];
}

class RecentTransaction extends Equatable {
  final String id;
  final String merchant;
  final double amount;
  final String currency;
  final String category;
  final String categoryName;
  final String sourceName;
  final DateTime captureDate;
  final bool isConfirmed;
  final bool needsReview;

  const RecentTransaction({
    required this.id,
    required this.merchant,
    required this.amount,
    required this.currency,
    required this.category,
    required this.categoryName,
    required this.sourceName,
    required this.captureDate,
    required this.isConfirmed,
    required this.needsReview,
  });

  @override
  List<Object?> get props => [id, merchant, amount];
}
