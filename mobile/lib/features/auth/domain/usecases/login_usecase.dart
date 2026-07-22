import 'package:dartz/dartz.dart';
import '../entities/auth_user.dart';
import '../repositories/auth_repository.dart';

class LoginParams {
  final String email;
  final String password;
  LoginParams({required this.email, required this.password});
}

class LoginUsecase {
  final AuthRepository _repository;
  LoginUsecase(this._repository);

  Future<Either<Failure, AuthUser>> call(LoginParams params) =>
      _repository.login(email: params.email, password: params.password);
}

class RegisterParams {
  final String name;
  final String email;
  final String password;
  final String currency;
  RegisterParams({required this.name, required this.email, required this.password, required this.currency});
}

class RegisterUsecase {
  final AuthRepository _repository;
  RegisterUsecase(this._repository);

  Future<Either<Failure, AuthUser>> call(RegisterParams params) =>
      _repository.register(name: params.name, email: params.email, password: params.password, currency: params.currency);
}
