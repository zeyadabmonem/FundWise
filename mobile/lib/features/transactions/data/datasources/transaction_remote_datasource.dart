import '../../../transactions/presentation/bloc/transaction_bloc.dart';
import '../../../../core/network/api_client.dart';

class TransactionRemoteDatasource {
  final ApiClient _apiClient;
  TransactionRemoteDatasource(this._apiClient);

  Future<List<Transaction>> getTransactions({int? month, int? year}) async {
    final now = DateTime.now();
    final response = await _apiClient.dio.get('/transactions', queryParameters: {
      'month': (month ?? now.month).toString(),
      'year': (year ?? now.year).toString(),
    });

    return (response.data as List).map((t) => _map(t)).toList();
  }

  Future<Transaction> createTransaction({
    required String merchant,
    required double amount,
    String? category,
    DateTime? captureDate,
    String? notes,
  }) async {
    final response = await _apiClient.dio.post('/transactions', data: {
      'merchant': merchant,
      'amount': amount,
      'category': category ?? 'Other',
      'captureDate': (captureDate ?? DateTime.now()).toIso8601String(),
      'notes': notes,
    });
    return _map(response.data);
  }

  Transaction _map(Map<String, dynamic> t) => Transaction(
        id: t['id'],
        merchant: t['merchant'],
        amount: (t['amount'] as num).toDouble(),
        currency: t['currency'] ?? 'EGP',
        category: t['category'],
        categoryName: t['categoryName'] ?? t['category'],
        source: t['source'] ?? 'Manual',
        sourceName: t['sourceName'] ?? t['source'] ?? 'Manual',
        captureDate: DateTime.parse(t['captureDate']),
        notes: t['notes'],
        isConfirmed: t['isConfirmed'] ?? true,
        needsReview: t['needsReview'] ?? false,
        confidenceScore: (t['confidenceScore'] as num?)?.toDouble() ?? 1.0,
      );
}
