import 'package:dartz/dartz.dart';
import '../../domain/entities/auth_user.dart';

abstract class AuthRepository {
  Future<Either<Failure, AuthUser>> login({required String email, required String password});
  Future<Either<Failure, AuthUser>> register({required String name, required String email, required String password, required String currency});
  Future<void> logout();
  Future<bool> isAuthenticated();
}
