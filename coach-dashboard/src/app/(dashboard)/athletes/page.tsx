'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader, DataTable, Column, StatusBadge, Pagination } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Plus, Search } from 'lucide-react';
import { useAthletes, useCreateAthlete } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { AthleteListItem } from '@/types';
import { format } from 'date-fns';
import { toast } from 'sonner';

export default function AthletesPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { coach } = useCoach();
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState<string>('');
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    goals: '',
    country: '',
  });

  const coachId = user?.coachId ?? coach?.id ?? '';

  const { data, isLoading } = useAthletes({
    coachId,
    status: status && status !== 'all' ? status : undefined,
    searchTerm: search || undefined,
    pageNumber: page,
    pageSize: 10,
  });

  const createAthlete = useCreateAthlete();

  const handleCreate = async () => {
    if (!coachId || !form.firstName.trim() || !form.lastName.trim() || !form.email.trim()) return;
    try {
      await createAthlete.mutateAsync({
        coachId,
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        phone: form.phone.trim() || undefined,
        goals: form.goals.trim() || undefined,
        country: form.country.trim() || undefined,
      });
      toast.success('Athlete added successfully');
      setDialogOpen(false);
      setForm({ firstName: '', lastName: '', email: '', phone: '', goals: '', country: '' });
    } catch {
      toast.error('Failed to add athlete');
    }
  };

  const columns: Column<AthleteListItem>[] = [
    {
      key: 'name',
      header: 'Name',
      cell: (row) => (
        <div>
          <p className="font-medium text-slate-900">
            {row.fullName}
          </p>
        </div>
      ),
    },
    {
      key: 'email',
      header: 'Email',
      cell: (row) => <span className="text-slate-600">{row.email}</span>,
    },
    {
      key: 'country',
      header: 'Country',
      cell: (row) => <span className="text-slate-600">{row.country || '—'}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      cell: (row) => <StatusBadge status={row.status} />,
    },
    {
      key: 'startDate',
      header: 'Start Date',
      cell: (row) => (
        <span className="text-slate-500 text-sm">
          {row.startDate ? format(new Date(row.startDate), 'MMM d, yyyy') : '—'}
        </span>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Athletes"
        description="Manage your coached athletes"
        actions={
          <Button onClick={() => setDialogOpen(true)}>
            <Plus className="w-4 h-4 mr-2" />
            Add Athlete
          </Button>
        }
      />

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <Input
            type="search"
            placeholder="Search by name or email..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="pl-10"
          />
        </div>
        <Select
          value={status}
          onValueChange={(value) => {
            setStatus(value);
            setPage(1);
          }}
        >
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            <SelectItem value="Active">Active</SelectItem>
            <SelectItem value="Inactive">Inactive</SelectItem>
            <SelectItem value="OnHold">On Hold</SelectItem>
            <SelectItem value="Graduated">Graduated</SelectItem>
            <SelectItem value="Suspended">Suspended</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <DataTable
        columns={columns}
        data={data?.items || []}
        isLoading={isLoading}
        emptyMessage="No athletes found."
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/athletes/${row.id}`)}
      />

      {/* Pagination */}
      {data && (
        <Pagination
          currentPage={data.pageNumber}
          totalPages={data.totalPages}
          onPageChange={setPage}
        />
      )}

      {/* Add Athlete Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Athlete</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>First Name *</Label>
                <Input
                  value={form.firstName}
                  onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                  placeholder="John"
                />
              </div>
              <div className="space-y-2">
                <Label>Last Name *</Label>
                <Input
                  value={form.lastName}
                  onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                  placeholder="Doe"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Email *</Label>
              <Input
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                placeholder="john@example.com"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Phone</Label>
                <Input
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                  placeholder="+1 234 567 890"
                />
              </div>
              <div className="space-y-2">
                <Label>Country</Label>
                <Input
                  value={form.country}
                  onChange={(e) => setForm({ ...form, country: e.target.value })}
                  placeholder="USA"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Goals</Label>
              <Textarea
                value={form.goals}
                onChange={(e) => setForm({ ...form, goals: e.target.value })}
                placeholder="Training goals..."
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              onClick={handleCreate}
              disabled={!form.firstName.trim() || !form.lastName.trim() || !form.email.trim() || createAthlete.isPending}
            >
              {createAthlete.isPending ? 'Adding...' : 'Add Athlete'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
