import 'package:dartz/dartz.dart';
import '../entities/auth_user.dart';
import '../repositories/auth_repository.dart';

class RegisterUsecase {
  final AuthRepository _repository;
  RegisterUsecase(this._repository);
}
