import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/transaction_bloc.dart';

class TransactionDetailPage extends StatefulWidget {
  final String transactionId;
  const TransactionDetailPage({super.key, required this.transactionId});

  @override
  State<TransactionDetailPage> createState() => _TransactionDetailPageState();
}

class _TransactionDetailPageState extends State<TransactionDetailPage> {
  Transaction? _transaction;

  @override
  void initState() {
    super.initState();
    final state = context.read<TransactionBloc>().state;
    if (state is TransactionLoaded) {
      _transaction = state.transactions.firstWhere(
        (t) => t.id == widget.transactionId,
        orElse: () => state.transactions.first,
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final t = _transaction;
    if (t == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Transaction')),
        body: const Center(child: Text('Transaction not found', style: TextStyle(color: AppColors.textSecondary))),
      );
    }

    final categoryColors = {
      'FoodAndDrink': AppColors.foodAndDrink,
      'Groceries': AppColors.groceries,
      'Transport': AppColors.transport,
      'BillsAndUtilities': AppColors.bills,
      'Shopping': AppColors.shopping,
      'Entertainment': AppColors.entertainment,
      'Health': AppColors.health,
      'Education': AppColors.education,
      'Transfer': AppColors.transfer,
      'Other': AppColors.other,
    };

    final color = categoryColors[t.category] ?? AppColors.other;

    return Scaffold(
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            expandedHeight: 200,
            pinned: true,
            backgroundColor: AppColors.background,
            leading: IconButton(
              icon: const Icon(Icons.arrow_back_ios, color: AppColors.textPrimary),
              onPressed: () => context.pop(),
            ),
            flexibleSpace: FlexibleSpaceBar(
              background: Container(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [color.withOpacity(0.3), AppColors.background],
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                  ),
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const SizedBox(height: 48),
                    Container(
                      width: 64,
                      height: 64,
                      decoration: BoxDecoration(
                        color: color.withOpacity(0.15),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Icon(Icons.store_rounded, color: color, size: 32),
                    ),
                    const SizedBox(height: 12),
                    Text(t.merchant, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
                    const SizedBox(height: 4),
                    Text(
                      '${t.currency} ${NumberFormat('#,##0.00').format(t.amount)}',
                      style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: color),
                    ),
                  ],
                ),
              ),
            ),
          ),
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _DetailRow('Category', t.categoryName),
                  _DetailRow('Source', t.sourceName),
                  _DetailRow('Date', DateFormat('dd MMM yyyy – hh:mm a').format(t.captureDate)),
                  _DetailRow('Status', t.isConfirmed ? '✅ Confirmed' : t.needsReview ? '⚠️ Needs Review' : '⏳ Pending'),
                  if (t.notes != null) _DetailRow('Notes', t.notes!),
                  _DetailRow('AI Confidence', '${(t.confidenceScore * 100).toStringAsFixed(0)}%'),
                  const SizedBox(height: 32),
                  if (!t.isConfirmed)
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        onPressed: () {
                          context.read<TransactionBloc>().add(TransactionConfirmRequested(t.id));
                          context.pop();
                        },
                        icon: const Icon(Icons.check_rounded),
                        label: const Text('Confirm Transaction'),
                      ),
                    ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  final String label;
  final String value;
  const _DetailRow(this.label, this.value);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 10),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(color: AppColors.textSecondary, fontSize: 14)),
          Text(value, style: const TextStyle(color: AppColors.textPrimary, fontSize: 14, fontWeight: FontWeight.w600)),
        ],
      ),
    );
  }
}
