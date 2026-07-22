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
      'fullName': name,
      'email': email,
      'password': password,
      'currency': currency,
    });
    return _mapUser(response.data);
  }

  AuthUser _mapUser(Map<String, dynamic> data) {
    return AuthUser(
      userId: data['userId'] ?? data['id'] ?? '',
      name: data['fullName'] ?? data['name'] ?? '',
      email: data['email'] ?? '',
      currency: data['currency'] ?? 'EGP',
      accessToken: data['accessToken'] ?? data['token'],
      refreshToken: data['refreshToken'],
    );
  }
}
