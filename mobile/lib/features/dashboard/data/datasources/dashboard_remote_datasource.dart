import '../../domain/entities/dashboard_entities.dart';
import '../../../../core/network/api_client.dart';
import '../../../../core/config/app_config.dart';

class DashboardRemoteDatasource {
  final ApiClient _apiClient;
  DashboardRemoteDatasource(this._apiClient);

  Future<DashboardSummary> getDashboardSummary({int? month, int? year}) async {
    final now = DateTime.now();
    final queryParams = <String, dynamic>{
      if (month != null) 'month': month,
      if (year != null) 'year': year,
    };

    final response = await _apiClient.dio.get(
      ApiEndpoints.dashboardSummary,
      queryParameters: queryParams,
    );

    final data = response.data as Map<String, dynamic>;
    return DashboardSummary(
      totalSpent: ((data['totalSpent'] ?? 0) as num).toDouble(),
      currency: data['currency']?.toString() ?? 'EGP',
      transactionCount: (data['transactionCount'] ?? 0) as int,
      month: (data['month'] ?? now.month) as int,
      year: (data['year'] ?? now.year) as int,
      categoryBreakdown: ((data['categoryBreakdown'] as List?) ?? [])
          .map((c) => CategoryBreakdown(
                category: c['category']?.toString() ?? '',
                categoryName: c['categoryName']?.toString() ?? 'Uncategorized',
                total: ((c['total'] ?? 0) as num).toDouble(),
                count: (c['count'] ?? 0) as int,
                percentage: ((c['percentage'] ?? 0) as num).toDouble(),
              ))
          .toList(),
      recentTransactions: ((data['recentTransactions'] as List?) ?? [])
          .map((t) => RecentTransaction(
                id: t['id']?.toString() ?? '',
                merchant: t['merchant']?.toString() ?? 'Expense',
                amount: ((t['amount'] ?? 0) as num).toDouble(),
                currency: t['currency']?.toString() ?? 'EGP',
                category: t['category']?.toString() ?? '',
                categoryName: t['categoryName']?.toString() ?? 'Uncategorized',
                sourceName: t['sourceName']?.toString() ?? t['source']?.toString() ?? 'Manual',
                captureDate: t['captureDate'] != null
                    ? DateTime.tryParse(t['captureDate'].toString()) ?? DateTime.now()
                    : DateTime.now(),
                isConfirmed: t['isConfirmed'] ?? true,
                needsReview: t['needsReview'] ?? false,
              ))
          .toList(),
    );
  }
}
