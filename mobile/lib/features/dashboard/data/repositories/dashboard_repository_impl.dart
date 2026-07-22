import 'package:dartz/dartz.dart';
import '../../domain/repositories/dashboard_repository.dart';
import '../../domain/entities/dashboard_entities.dart';
import '../../../../features/auth/domain/entities/auth_user.dart';
import '../datasources/dashboard_remote_datasource.dart';

class DashboardRepositoryImpl implements DashboardRepository {
  final DashboardRemoteDatasource _remote;
  DashboardRepositoryImpl(this._remote);

  @override
  Future<Either<Failure, DashboardSummary>> getDashboardSummary({int? month, int? year}) async {
    try {
      final summary = await _remote.getDashboardSummary(month: month, year: year);
      return Right(summary);
    } catch (e) {
      return Left(Failure(message: 'Failed to load dashboard data.'));
    }
  }
}
