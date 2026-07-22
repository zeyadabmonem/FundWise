import 'package:dartz/dartz.dart';
import '../../domain/entities/dashboard_entities.dart';
import '../../../../features/auth/domain/entities/auth_user.dart';

abstract class DashboardRepository {
  Future<Either<Failure, DashboardSummary>> getDashboardSummary({int? month, int? year});
}

class DashboardParams {
  final int? month;
  final int? year;
  DashboardParams({this.month, this.year});
}

class GetDashboardSummaryUsecase {
  final DashboardRepository _repository;
  GetDashboardSummaryUsecase(this._repository);

  Future<Either<Failure, DashboardSummary>> call(DashboardParams params) =>
      _repository.getDashboardSummary(month: params.month, year: params.year);
}
