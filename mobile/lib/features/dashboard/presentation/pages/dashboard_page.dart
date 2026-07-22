import 'package:fl_chart/fl_chart.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import '../../../../core/themes/app_theme.dart';
import '../bloc/dashboard_bloc.dart';
import '../../domain/entities/dashboard_entities.dart';

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key});

  @override
  State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> {
  @override
  void initState() {
    super.initState();
    context.read<DashboardBloc>().add(DashboardLoadRequested());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: BlocBuilder<DashboardBloc, DashboardState>(
        builder: (context, state) {
          return RefreshIndicator(
            color: AppColors.primary,
            backgroundColor: AppColors.surface,
            onRefresh: () async {
              context.read<DashboardBloc>().add(DashboardRefreshRequested());
            },
            child: CustomScrollView(
              slivers: [
                _buildAppBar(context),
                if (state is DashboardLoading)
                  const SliverFillRemaining(child: Center(child: CircularProgressIndicator(color: AppColors.primary)))
                else if (state is DashboardError)
                  SliverFillRemaining(child: _buildError(state.message))
                else if (state is DashboardLoaded) ...[
                  SliverToBoxAdapter(child: _buildTotalCard(state.summary)),
                  SliverToBoxAdapter(child: _buildCategoryChart(state.summary.categoryBreakdown)),
                  SliverToBoxAdapter(child: _buildRecentTransactions(state.summary.recentTransactions)),
                ],
              ],
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => Navigator.pushNamed(context, '/transactions/add'),
        backgroundColor: AppColors.primary,
        icon: const Icon(Icons.add, color: Colors.white),
        label: const Text('Add', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
      ),
    );
  }

  SliverAppBar _buildAppBar(BuildContext context) {
    return SliverAppBar(
      expandedHeight: 120,
      pinned: true,
      backgroundColor: AppColors.background,
      flexibleSpace: FlexibleSpaceBar(
        title: RichText(
          text: const TextSpan(
            text: 'FundWise ',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: AppColors.textPrimary),
            children: [
              TextSpan(
                text: 'AI',
                style: TextStyle(color: AppColors.primary),
              ),
            ],
          ),
        ),
        titlePadding: const EdgeInsets.only(left: 20, bottom: 16),
      ),
      actions: [
        IconButton(
          icon: const Icon(Icons.notifications_none_rounded, color: AppColors.textSecondary),
          onPressed: () {},
        ),
        const SizedBox(width: 8),
      ],
    );
  }

  Widget _buildTotalCard(DashboardSummary summary) {
    final monthName = DateFormat('MMMM yyyy').format(DateTime(summary.year, summary.month));
    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
      child: Container(
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            colors: [Color(0xFF5B2D8E), AppColors.primary],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(20),
          boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.3), blurRadius: 20, offset: const Offset(0, 8))],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(monthName, style: const TextStyle(color: Colors.white70, fontSize: 13, fontWeight: FontWeight.w500)),
            const SizedBox(height: 8),
            const Text('Total Spent', style: TextStyle(color: Colors.white70, fontSize: 14)),
            const SizedBox(height: 4),
            Text(
              '${summary.currency} ${NumberFormat('#,##0.00').format(summary.totalSpent)}',
              style: const TextStyle(color: Colors.white, fontSize: 32, fontWeight: FontWeight.bold, letterSpacing: -1),
            ),
            const SizedBox(height: 12),
            Row(children: [
              const Icon(Icons.receipt_long_rounded, color: Colors.white60, size: 16),
              const SizedBox(width: 6),
              Text('${summary.transactionCount} transactions', style: const TextStyle(color: Colors.white70, fontSize: 13)),
            ]),
          ],
        ),
      ),
    );
  }

  Widget _buildCategoryChart(List<CategoryBreakdown> categories) {
    if (categories.isEmpty) return const SizedBox.shrink();

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

    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: AppColors.cardBorder),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Spending by Category', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
            const SizedBox(height: 20),
            SizedBox(
              height: 200,
              child: PieChart(
                PieChartData(
                  sections: categories.map((c) {
                    final color = categoryColors[c.category] ?? AppColors.other;
                    return PieChartSectionData(
                      value: c.total,
                      color: color,
                      radius: 60,
                      showTitle: false,
                    );
                  }).toList(),
                  centerSpaceRadius: 50,
                  sectionsSpace: 3,
                ),
              ),
            ),
            const SizedBox(height: 16),
            Wrap(
              spacing: 12,
              runSpacing: 8,
              children: categories.take(5).map((c) {
                final color = categoryColors[c.category] ?? AppColors.other;
                return Row(mainAxisSize: MainAxisSize.min, children: [
                  Container(width: 10, height: 10, decoration: BoxDecoration(color: color, shape: BoxShape.circle)),
                  const SizedBox(width: 4),
                  Text('${c.categoryName} ${c.percentage.toStringAsFixed(0)}%',
                      style: const TextStyle(fontSize: 12, color: AppColors.textSecondary)),
                ]);
              }).toList(),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRecentTransactions(List<RecentTransaction> transactions) {
    if (transactions.isEmpty) {
      return Padding(
        padding: const EdgeInsets.all(24),
        child: Center(
          child: Column(children: const [
            Icon(Icons.receipt_long_outlined, size: 48, color: AppColors.textMuted),
            SizedBox(height: 12),
            Text('No transactions yet', style: TextStyle(color: AppColors.textMuted, fontSize: 15)),
            SizedBox(height: 4),
            Text('Tap + to add your first one', style: TextStyle(color: AppColors.textMuted, fontSize: 13)),
          ]),
        ),
      );
    }

    return Padding(
      padding: const EdgeInsets.fromLTRB(16, 0, 16, 100),
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: AppColors.cardBorder),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Recent Transactions', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppColors.textPrimary)),
            const SizedBox(height: 12),
            ...transactions.map((t) => _buildTransactionItem(t)),
          ],
        ),
      ),
    );
  }

  Widget _buildTransactionItem(RecentTransaction t) {
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

    final categoryIcons = {
      'FoodAndDrink': Icons.restaurant_rounded,
      'Groceries': Icons.shopping_cart_rounded,
      'Transport': Icons.directions_car_rounded,
      'BillsAndUtilities': Icons.receipt_rounded,
      'Shopping': Icons.shopping_bag_rounded,
      'Entertainment': Icons.movie_rounded,
      'Health': Icons.medical_services_rounded,
      'Education': Icons.school_rounded,
      'Transfer': Icons.swap_horiz_rounded,
      'Other': Icons.category_rounded,
    };

    final color = categoryColors[t.category] ?? AppColors.other;
    final icon = categoryIcons[t.category] ?? Icons.category_rounded;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(children: [
        Container(
          width: 44,
          height: 44,
          decoration: BoxDecoration(color: color.withOpacity(0.15), borderRadius: BorderRadius.circular(12)),
          child: Icon(icon, color: color, size: 22),
        ),
        const SizedBox(width: 12),
        Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(t.merchant, style: const TextStyle(color: AppColors.textPrimary, fontWeight: FontWeight.w600, fontSize: 14), overflow: TextOverflow.ellipsis),
          Text(t.categoryName, style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
        ])),
        Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
          Text('${t.currency} ${NumberFormat('#,##0.00').format(t.amount)}',
              style: const TextStyle(color: AppColors.textPrimary, fontWeight: FontWeight.bold, fontSize: 14)),
          if (t.needsReview)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(color: AppColors.warning.withOpacity(0.2), borderRadius: BorderRadius.circular(6)),
              child: const Text('Review', style: TextStyle(color: AppColors.warning, fontSize: 10, fontWeight: FontWeight.bold)),
            )
          else if (!t.isConfirmed)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
              decoration: BoxDecoration(color: AppColors.primary.withOpacity(0.2), borderRadius: BorderRadius.circular(6)),
              child: const Text('Pending', style: TextStyle(color: AppColors.primary, fontSize: 10, fontWeight: FontWeight.bold)),
            ),
        ]),
      ]),
    );
  }

  Widget _buildError(String message) {
    return Center(child: Padding(
      padding: const EdgeInsets.all(32),
      child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
        const Icon(Icons.wifi_off_rounded, size: 48, color: AppColors.textMuted),
        const SizedBox(height: 12),
        Text(message, style: const TextStyle(color: AppColors.textMuted, fontSize: 15), textAlign: TextAlign.center),
        const SizedBox(height: 16),
        ElevatedButton(
          onPressed: () => context.read<DashboardBloc>().add(DashboardLoadRequested()),
          child: const Text('Retry'),
        ),
      ]),
    ));
  }
}
