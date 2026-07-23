import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../../domain/entities/auth_user.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/auth_remote_datasource.dart';

class AuthRepositoryImpl implements AuthRepository {
  final AuthRemoteDatasource _remote;
  final FlutterSecureStorage _storage;

  AuthRepositoryImpl(this._remote, this._storage);

  @override
  Future<Either<Failure, AuthUser>> login({required String email, required String password}) async {
    try {
      final user = await _remote.login(email: email, password: password);
      // Persist tokens
      await _storage.write(key: 'access_token', value: user.accessToken);
      await _storage.write(key: 'refresh_token', value: user.refreshToken);
      return Right(user);
    } catch (e) {
      return Left(Failure(message: _mapError(e)));
    }
  }

  @override
  Future<Either<Failure, AuthUser>> register({
    required String name,
    required String email,
    required String password,
    required String currency,
  }) async {
    try {
      final user = await _remote.register(name: name, email: email, password: password, currency: currency);
      await _storage.write(key: 'access_token', value: user.accessToken);
      await _storage.write(key: 'refresh_token', value: user.refreshToken);
      return Right(user);
    } catch (e) {
      return Left(Failure(message: _mapError(e)));
    }
  }

  @override
  Future<void> logout() async {
    await _storage.deleteAll();
  }

  @override
  Future<bool> isAuthenticated() async {
    final token = await _storage.read(key: 'access_token');
    return token != null;
  }

  String _mapError(dynamic e) {
    if (e is DioException) {
      final data = e.response?.data;
      if (data is Map && data.containsKey('message')) {
        return data['message'] as String;
      }
      switch (e.response?.statusCode) {
        case 401: return 'Invalid email or password.';
        case 409: return 'An account with this email already exists.';
        case 422: return 'Please fill all required fields correctly.';
      }
    }
    return 'Something went wrong. Please try again.';
  }
}
