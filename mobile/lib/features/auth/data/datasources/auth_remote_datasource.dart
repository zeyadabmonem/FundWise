import 'package:dio/dio.dart';
import '../../domain/entities/auth_user.dart';
import '../../../../core/network/api_client.dart';

class AuthRemoteDatasource {
  final ApiClient _apiClient;
  AuthRemoteDatasource(this._apiClient);

  Future<AuthUser> login({required String email, required String password}) async {
    final response = await _apiClient.dio.post('/auth/login', data: {
      'email': email,
      'password': password,
    });
    return _mapUser(response.data);
  }

  Future<AuthUser> register({
    required String name,
    required String email,
    required String password,
    required String currency,
  }) async {
    final response = await _apiClient.dio.post('/auth/register', data: {
      'name': name,
      'email': email,
      'password': password,
      'currency': currency,
    });
    return _mapUser(response.data);
  }

  AuthUser _mapUser(Map<String, dynamic> data) {
    final tokens = data['tokens'] as Map<String, dynamic>?;
    final user = data['user'] as Map<String, dynamic>?;

    final accessToken = tokens?['accessToken'] ?? data['accessToken'] ?? data['token'];
    final refreshToken = tokens?['refreshToken'] ?? data['refreshToken'];

    final userId = user?['id'] ?? user?['userId'] ?? data['userId'] ?? data['id'] ?? '';
    final name = user?['name'] ?? user?['fullName'] ?? data['fullName'] ?? data['name'] ?? '';
    final email = user?['email'] ?? data['email'] ?? '';
    final currency = user?['currency'] ?? data['currency'] ?? 'EGP';

    return AuthUser(
      userId: userId.toString(),
      name: name.toString(),
      email: email.toString(),
      currency: currency.toString(),
      accessToken: accessToken?.toString() ?? '',
      refreshToken: refreshToken?.toString(),
    );
  }
}
