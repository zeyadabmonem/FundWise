import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../config/app_config.dart';

class ApiClient {
  late final Dio _dio;
  final FlutterSecureStorage _secureStorage;

  ApiClient(this._secureStorage) {
    _dio = Dio(BaseOptions(
      baseUrl: AppConfig.baseUrl,
      connectTimeout: AppConfig.requestTimeout,
      receiveTimeout: AppConfig.requestTimeout,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ));

    _dio.interceptors.add(_AuthInterceptor(_secureStorage, _dio));
    _dio.interceptors.add(LogInterceptor(
      request: true,
      responseBody: true,
      requestBody: true,
      error: true,
    ));
  }

  Dio get dio => _dio;
}

class _AuthInterceptor extends Interceptor {
  final FlutterSecureStorage _storage;
  final Dio _dio;

  _AuthInterceptor(this._storage, this._dio);

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _storage.read(key: 'access_token');
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    if (err.response?.statusCode == 401) {
      // Attempt silent refresh
      try {
        final refreshToken = await _storage.read(key: 'refresh_token');
        if (refreshToken != null) {
          final response = await _dio.post('/auth/refresh', data: {
            'refreshToken': refreshToken,
          });

          final newAccessToken = response.data['accessToken'];
          await _storage.write(key: 'access_token', value: newAccessToken);

          // Retry the failed request
          err.requestOptions.headers['Authorization'] = 'Bearer $newAccessToken';
          final retried = await _dio.fetch(err.requestOptions);
          return handler.resolve(retried);
        }
      } catch (_) {
        // Refresh failed — clear tokens and send to login
        await _storage.deleteAll();
      }
    }
    handler.next(err);
  }
}
