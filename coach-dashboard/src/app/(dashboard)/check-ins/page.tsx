'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader, DataTable, Column, Pagination, StatusBadge } from '@/components/common';
import { Badge } from '@/components/ui/badge';
import { useCoachCheckIns } from '@/hooks';
import { useAuth } from '@/providers';
import { format } from 'date-fns';
import { CheckInDto } from '@/types/checkIn';
import { Eye, CheckCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

export default function CheckInsPage() {
  const router = useRouter();
  const { user } = useAuth();
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<'all' | 'pending' | 'reviewed'>('pending');

  const { data, isLoading } = useCoachCheckIns({
    coachId: user?.id || '',
    isReviewed: filter === 'pending' ? false : filter === 'reviewed' ? true : undefined,
    page,
    size: 10,
    enabled: !!user?.id,
  });

  const columns: Column<CheckInDto>[] = [
    {
      key: 'date',
      header: 'Date',
      cell: (row) => format(new Date(row.checkInDate), 'MMM d, yyyy'),
    },
    {
      key: 'athlete',
      header: 'Athlete',
      cell: (row) => row.athleteName,
    },
    {
      key: 'weight',
      header: 'Weight',
      cell: (row) => row.weight ? `${row.weight} ${row.weightUnit || 'kg'}` : '-',
    },
    {
      key: 'energy',
      header: 'Energy',
      cell: (row) => row.energyLevel ? `${row.energyLevel}/10` : '-',
    },
    {
      key: 'status',
      header: 'Status',
      cell: (row) => (
        <StatusBadge 
          status={row.reviewedAt ? 'active' : 'pending'} 
        />
      ),
    },
    {
      key: 'photos',
      header: 'Photos',
      cell: (row) => (
        <Badge variant={row.photoUrls?.length > 0 ? "secondary" : "outline"}>
          {row.photoUrls?.length || 0}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: 'Actions',
      cell: (row) => (
        <Button 
          variant="ghost" 
          size="sm" 
          className="h-8 group-hover:bg-blue-50"
          onClick={(e) => {
            e.stopPropagation();
            router.push(`/check-ins/${row.id}`);
          }}
        >
          {row.reviewedAt ? <Eye className="w-4 h-4 text-slate-500" /> : <CheckCircle className="w-4 h-4 text-blue-600" />}
          <span className="ml-2">{row.reviewedAt ? 'View' : 'Review'}</span>
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader 
        title="Check-ins" 
        description="Review athlete progress reports and provide feedback."
      />

      {/* Filters */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center p-4 bg-white rounded-lg shadow-sm border border-slate-200">
        <div className="w-full md:w-64">
          <Select 
            value={filter} 
            onValueChange={(val) => {
              setFilter(val as 'all' | 'pending' | 'reviewed');
              setPage(1);
            }}
          >
            <SelectTrigger>
              <SelectValue placeholder="Filter by status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="pending">Pending Review</SelectItem>
              <SelectItem value="reviewed">Reviewed</SelectItem>
              <SelectItem value="all">All Check-ins</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="bg-white border rounded-xl shadow-sm border-slate-200 overflow-hidden">
        <DataTable
          columns={columns}
          data={data?.items || []}
          isLoading={isLoading}
          emptyMessage="No check-ins found."
          rowKey={(row) => row.id}
          onRowClick={(row) => router.push(`/check-ins/${row.id}`)}
        />
        
        {data && data.totalPages > 1 && (
          <div className="p-4 border-t border-slate-200">
            <Pagination
              currentPage={data.pageNumber}
              totalPages={data.totalPages}
              onPageChange={setPage}
            />
          </div>
        )}
      </div>
    </div>
  );
}
