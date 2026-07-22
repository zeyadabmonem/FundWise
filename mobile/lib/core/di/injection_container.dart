import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:get_it/get_it.dart';
import 'package:hive_flutter/hive_flutter.dart';

import '../config/app_config.dart';
import '../../features/auth/data/datasources/auth_remote_datasource.dart';
import '../../features/auth/data/repositories/auth_repository_impl.dart';
import '../../features/auth/domain/repositories/auth_repository.dart';
import '../../features/auth/domain/usecases/login_usecase.dart';
import '../../features/auth/domain/usecases/register_usecase.dart';
import '../../features/auth/presentation/bloc/auth_bloc.dart';
import '../../features/dashboard/data/datasources/dashboard_remote_datasource.dart';
import '../../features/dashboard/data/repositories/dashboard_repository_impl.dart';
import '../../features/dashboard/domain/repositories/dashboard_repository.dart';
import '../../features/dashboard/domain/usecases/get_dashboard_summary_usecase.dart';
import '../../features/dashboard/presentation/bloc/dashboard_bloc.dart';
import '../../features/transactions/data/datasources/transaction_remote_datasource.dart';
import '../../features/transactions/data/repositories/transaction_repository_impl.dart';
import '../../features/transactions/domain/repositories/transaction_repository.dart';
import '../../features/transactions/domain/usecases/create_transaction_usecase.dart';
import '../../features/transactions/domain/usecases/get_transactions_usecase.dart';
import '../../features/transactions/presentation/bloc/transaction_bloc.dart';
import '../network/api_client.dart';

final GetIt sl = GetIt.instance;

Future<void> configureDependencies() async {
  // ─── External ───────────────────────────────────────────────────
  sl.registerLazySingleton(() => const FlutterSecureStorage());
  sl.registerLazySingleton(() => Hive.box('fundwise_cache'));

  // ─── Network ────────────────────────────────────────────────────
  sl.registerLazySingleton<ApiClient>(() => ApiClient(sl()));

  // ─── Auth Feature ────────────────────────────────────────────────
  sl.registerLazySingleton<AuthRemoteDatasource>(
    () => AuthRemoteDatasource(sl<ApiClient>()),
  );
  sl.registerLazySingleton<AuthRepository>(
    () => AuthRepositoryImpl(sl(), sl()),
  );
  sl.registerLazySingleton(() => LoginUsecase(sl()));
  sl.registerLazySingleton(() => RegisterUsecase(sl()));
  sl.registerFactory(() => AuthBloc(loginUsecase: sl(), registerUsecase: sl()));

  // ─── Dashboard Feature ────────────────────────────────────────────
  sl.registerLazySingleton<DashboardRemoteDatasource>(
    () => DashboardRemoteDatasource(sl<ApiClient>()),
  );
  sl.registerLazySingleton<DashboardRepository>(
    () => DashboardRepositoryImpl(sl()),
  );
  sl.registerLazySingleton(() => GetDashboardSummaryUsecase(sl()));
  sl.registerFactory(() => DashboardBloc(getDashboardSummary: sl()));

  // ─── Transactions Feature ─────────────────────────────────────────
  sl.registerLazySingleton<TransactionRemoteDatasource>(
    () => TransactionRemoteDatasource(sl<ApiClient>()),
  );
  sl.registerLazySingleton<TransactionRepository>(
    () => TransactionRepositoryImpl(sl()),
  );
  sl.registerLazySingleton(() => GetTransactionsUsecase(sl()));
  sl.registerLazySingleton(() => CreateTransactionUsecase(sl()));
  sl.registerFactory(() => TransactionBloc(
    getTransactions: sl(),
    createTransaction: sl(),
  ));
}
