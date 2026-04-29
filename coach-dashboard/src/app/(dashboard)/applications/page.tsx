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
import { Plus, Search } from 'lucide-react';
import { useApplications, useCreateApplication } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { ApplicationListItem } from '@/types';
import { format } from 'date-fns';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { toast } from 'sonner';

export default function ApplicationsPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { coach } = useCoach();
  const coachId = user?.coachId ?? coach?.id ?? '';
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
    message: '',
  });

  const { data, isLoading } = useApplications({
    coachId,
    status: status && status !== 'all' ? status : undefined,
    searchTerm: search || undefined,
    pageNumber: page,
    pageSize: 10,
  });

  const createApplication = useCreateApplication();

  const handleCreate = async () => {
    if (!coachId || !form.firstName.trim() || !form.lastName.trim() || !form.email.trim()) return;
    try {
      await createApplication.mutateAsync({
        coachId,
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        phone: form.phone.trim() || undefined,
        goals: form.goals.trim() || undefined,
        message: form.message.trim() || undefined,
      });
      toast.success('Application created successfully');
      setDialogOpen(false);
      setForm({ firstName: '', lastName: '', email: '', phone: '', goals: '', message: '' });
    } catch {
      toast.error('Failed to create application');
    }
  };

  const columns: Column<ApplicationListItem>[] = [
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
      key: 'status',
      header: 'Status',
      cell: (row) => <StatusBadge status={row.status} />,
    },
    {
      key: 'createdAt',
      header: 'Submitted',
      cell: (row) => (
        <span className="text-slate-500 text-sm">
          {format(new Date(row.createdAt), 'MMM d, yyyy')}
        </span>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Applications"
        description="Manage coaching applications from potential athletes"
        actions={
          <Button onClick={() => setDialogOpen(true)}>
            <Plus className="w-4 h-4 mr-2" />
            New Application
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
            <SelectItem value="Pending">Pending</SelectItem>
            <SelectItem value="UnderReview">Under Review</SelectItem>
            <SelectItem value="Accepted">Accepted</SelectItem>
            <SelectItem value="Rejected">Rejected</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <DataTable
        columns={columns}
        data={data?.items || []}
        isLoading={isLoading}
        emptyMessage="No applications found."
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/applications/${row.id}`)}
      />

      {/* Pagination */}
      {data && (
        <Pagination
          currentPage={data.pageNumber}
          totalPages={data.totalPages}
          onPageChange={setPage}
        />
      )}

      {/* New Application Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>New Application</DialogTitle>
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
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Email *</Label>
                <Input
                  type="email"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  placeholder="john@example.com"
                />
              </div>
              <div className="space-y-2">
                <Label>Phone</Label>
                <Input
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                  placeholder="+1 234 567 890"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Goals</Label>
              <Input
                value={form.goals}
                onChange={(e) => setForm({ ...form, goals: e.target.value })}
                placeholder="Training goals"
              />
            </div>
            <div className="space-y-2">
              <Label>Message</Label>
              <Textarea
                value={form.message}
                onChange={(e) => setForm({ ...form, message: e.target.value })}
                placeholder="Why do you want to join?"
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button
              onClick={handleCreate}
              disabled={!form.firstName.trim() || !form.lastName.trim() || !form.email.trim() || createApplication.isPending}
            >
              {createApplication.isPending ? 'Creating...' : 'Create Application'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
