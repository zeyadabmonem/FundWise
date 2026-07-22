import 'package:equatable/equatable.dart';

class AuthUser extends Equatable {
  final String userId;
  final String name;
  final String email;
  final String currency;
  final String? accessToken;
  final String? refreshToken;

  const AuthUser({
    required this.userId,
    required this.name,
    required this.email,
    required this.currency,
    this.accessToken,
    this.refreshToken,
  });

  @override
  List<Object?> get props => [userId, email];
}

class Failure extends Equatable {
  final String message;
  final int? statusCode;

  const Failure({required this.message, this.statusCode});

  @override
  List<Object?> get props => [message, statusCode];
}
