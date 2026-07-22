import '../../domain/entities/dashboard_entities.dart';
import '../../../../core/network/api_client.dart';

class DashboardRemoteDatasource {
  final ApiClient _apiClient;
  DashboardRemoteDatasource(this._apiClient);

  Future<DashboardSummary> getDashboardSummary({int? month, int? year}) async {
    final now = DateTime.now();
    final queryParams = {
      'month': (month ?? now.month).toString(),
      'year': (year ?? now.year).toString(),
    };

    final response = await _apiClient.dio.get(
      '/transactions/dashboard',
      queryParameters: queryParams,
    );

    final data = response.data as Map<String, dynamic>;
    return DashboardSummary(
      totalSpent: (data['totalSpent'] as num).toDouble(),
      currency: data['currency'] ?? 'EGP',
      transactionCount: data['transactionCount'] as int,
      month: data['month'] as int,
      year: data['year'] as int,
      categoryBreakdown: (data['categoryBreakdown'] as List)
          .map((c) => CategoryBreakdown(
                category: c['category'],
                categoryName: c['categoryName'],
                total: (c['total'] as num).toDouble(),
                count: c['count'] as int,
                percentage: (c['percentage'] as num).toDouble(),
              ))
          .toList(),
      recentTransactions: (data['recentTransactions'] as List)
          .map((t) => RecentTransaction(
                id: t['id'],
                merchant: t['merchant'],
                amount: (t['amount'] as num).toDouble(),
                currency: t['currency'] ?? 'EGP',
                category: t['category'],
                categoryName: t['categoryName'],
                sourceName: t['sourceName'] ?? t['source'] ?? '',
                captureDate: DateTime.parse(t['captureDate']),
                isConfirmed: t['isConfirmed'] ?? true,
                needsReview: t['needsReview'] ?? false,
              ))
          .toList(),
    );
  }
}
