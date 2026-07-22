import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../domain/usecases/login_usecase.dart';
import '../../domain/usecases/register_usecase.dart';

// ─── Events ────────────────────────────────────────────────────────────────
sealed class AuthEvent extends Equatable {
  @override
  List<Object> get props => [];
}

class AuthLoginRequested extends AuthEvent {
  final String email;
  final String password;
  AuthLoginRequested({required this.email, required this.password});
  @override
  List<Object> get props => [email, password];
}

class AuthRegisterRequested extends AuthEvent {
  final String name;
  final String email;
  final String password;
  final String currency;
  AuthRegisterRequested({required this.name, required this.email, required this.password, required this.currency});
  @override
  List<Object> get props => [name, email, password, currency];
}

class AuthLogoutRequested extends AuthEvent {}

// ─── States ────────────────────────────────────────────────────────────────
sealed class AuthState extends Equatable {
  @override
  List<Object?> get props => [];
}

class AuthInitial extends AuthState {}
class AuthLoading extends AuthState {}
class AuthAuthenticated extends AuthState {
  final String userId;
  final String name;
  final String email;
  AuthAuthenticated({required this.userId, required this.name, required this.email});
  @override
  List<Object?> get props => [userId, name, email];
}
class AuthUnauthenticated extends AuthState {}
class AuthFailure extends AuthState {
  final String message;
  AuthFailure(this.message);
  @override
  List<Object?> get props => [message];
}

// ─── BLoC ────────────────────────────────────────────────────────────────
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final LoginUsecase loginUsecase;
  final RegisterUsecase registerUsecase;

  AuthBloc({required this.loginUsecase, required this.registerUsecase})
      : super(AuthInitial()) {
    on<AuthLoginRequested>(_onLoginRequested);
    on<AuthRegisterRequested>(_onRegisterRequested);
    on<AuthLogoutRequested>(_onLogoutRequested);
  }

  Future<void> _onLoginRequested(AuthLoginRequested event, Emitter<AuthState> emit) async {
    emit(AuthLoading());
    final result = await loginUsecase(LoginParams(email: event.email, password: event.password));
    result.fold(
      (failure) => emit(AuthFailure(failure.message)),
      (user) => emit(AuthAuthenticated(userId: user.userId, name: user.name, email: user.email)),
    );
  }

  Future<void> _onRegisterRequested(AuthRegisterRequested event, Emitter<AuthState> emit) async {
    emit(AuthLoading());
    final result = await registerUsecase(RegisterParams(
      name: event.name,
      email: event.email,
      password: event.password,
      currency: event.currency,
    ));
    result.fold(
      (failure) => emit(AuthFailure(failure.message)),
      (user) => emit(AuthAuthenticated(userId: user.userId, name: user.name, email: user.email)),
    );
  }

  Future<void> _onLogoutRequested(AuthLogoutRequested event, Emitter<AuthState> emit) async {
    emit(AuthUnauthenticated());
  }
}
